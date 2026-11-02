// using System.Security.Claims;
// using System.Security.Cryptography;
// using Microsoft.AspNetCore.Authentication;
// using Microsoft.AspNetCore.Authentication.Cookies;
// using Microsoft.AspNetCore.Authorization;
// using Microsoft.AspNetCore.Mvc;
// using Microsoft.Data.SqlClient;
// using FoundationManagement.Models;

// namespace FoundationManagement.Controllers
// {
//     public class AccountController : Controller
//     {
//         private readonly string _connectionString;

//         public AccountController(IConfiguration configuration)
//         {
//             _connectionString = configuration.GetConnectionString("FoundationDb")
//                 ?? throw new InvalidOperationException("Connection string 'FoundationDb' not found in appsettings.json.");
//         }

//         // =====================================================================
//         // WELCOME
//         // =====================================================================
//         [HttpGet]
//         public IActionResult Welcome() => View();

//         // =====================================================================
//         // LOGIN / LOGOUT
//         // =====================================================================
//         [HttpGet]
//         public IActionResult Login()
//         {
//             if (TempData["LoginMessage"] is string msg) ViewBag.LoginMessage = msg;
//             return View();
//         }

//         [HttpPost]
//         [ValidateAntiForgeryToken]
//         public async Task<IActionResult> Login(LoginViewModel model)
//         {
//             if (!ModelState.IsValid)
//             {
//                 ViewBag.LoginError = "Please enter your username and password.";
//                 return View(model);
//             }

//             // Anyone with an account can log in. Staff/Manager rows also have a STAFF record.
//             const string sql = @"
//                 SELECT p.Person_ID, p.FName, p.LName, p.PasswordHash, p.Role,
//                        p.FoundationID, p.MustChangePassword, s.Position
//                 FROM PERSONS p
//                 LEFT JOIN STAFF s ON p.Person_ID = s.Person_ID
//                 WHERE p.Username = @Username";

//             string? storedHash = null;
//             string fName = "", lName = "", position = "", role = "Staff";
//             int personId = 0;
//             int? foundationId = null;
//             bool mustChange = false;

//             using (var connection = new SqlConnection(_connectionString))
//             {
//                 connection.Open();
//                 using var command = new SqlCommand(sql, connection);
//                 command.Parameters.AddWithValue("@Username", model.Username!.Trim().ToLowerInvariant());

//                 using var reader = command.ExecuteReader();
//                 if (reader.Read())
//                 {
//                     personId = reader.GetInt32(reader.GetOrdinal("Person_ID"));
//                     fName = reader["FName"]?.ToString() ?? "";
//                     lName = reader["LName"]?.ToString() ?? "";
//                     role = reader["Role"]?.ToString() ?? "Staff";
//                     position = reader["Position"] == DBNull.Value ? role : reader["Position"].ToString() ?? role;
//                     mustChange = reader["MustChangePassword"] != DBNull.Value && (bool)reader["MustChangePassword"];

//                     int fo = reader.GetOrdinal("FoundationID");
//                     if (!reader.IsDBNull(fo)) foundationId = reader.GetInt32(fo);

//                     int ho = reader.GetOrdinal("PasswordHash");
//                     storedHash = reader.IsDBNull(ho) ? null : reader.GetString(ho);
//                 }
//             }

//             // Same message whether the username is unknown or the password is wrong.
//             if (storedHash == null || !PasswordHasher.Verify(model.Password!, storedHash))
//             {
//                 ViewBag.LoginError = "Invalid username or password.";
//                 return View(model);
//             }

//             var claims = new List<Claim>
//             {
//                 new Claim(ClaimTypes.Name, model.Username!.Trim().ToLowerInvariant()),
//                 new Claim(ClaimTypes.NameIdentifier, personId.ToString()),
//                 new Claim(ClaimTypes.Role, role),
//                 new Claim("FullName", $"{fName} {lName}".Trim()),
//                 new Claim("Position", position),
//                 new Claim("PersonId", personId.ToString())
//             };
//             if (foundationId.HasValue)
//                 claims.Add(new Claim("FoundationId", foundationId.Value.ToString()));

//             var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
//             await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

//             if (mustChange)
//                 return RedirectToAction("ChangePassword", new { first = true });

//             return RedirectByRole(role);
//         }

//         public async Task<IActionResult> Logout()
//         {
//             await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
//             return RedirectToAction("Welcome");
//         }

//         // =====================================================================
//         // REGISTER
//         // =====================================================================
//         [HttpGet]
//         public IActionResult Register(string tab = "donor")
//             => View(new RegisterPageViewModel { ActiveTab = tab });

//         [HttpPost, ValidateAntiForgeryToken]
//         public IActionResult RegisterDonor([Bind(Prefix = "Donor")] RegisterDonorViewModel model)
//         {
//             var page = new RegisterPageViewModel { ActiveTab = "donor", Donor = model };

//             var email = (model.Email ?? "").Trim().ToLowerInvariant();
//             if (UsernameExists(email))
//                 ModelState.AddModelError("Donor.Email", "An account with this email already exists.");

//             if (!ModelState.IsValid) return View("Register", page);

//             var (first, last) = SplitName(model.FullName);

//             using var connection = new SqlConnection(_connectionString);
//             connection.Open();
//             using var tx = connection.BeginTransaction();
//             try
//             {
//                 // 1. The person (login account)
//                 int personId = InsertPerson(connection, tx, first, last, email, model.Phone, model.Password, "Donor", false);

//                 // 2. The donor record, linked to the person
//                 using (var cmd = new SqlCommand(
//                     "INSERT INTO DONOR (Person_ID, Donor_Type) VALUES (@Id, @Type)", connection, tx))
//                 {
//                     cmd.Parameters.AddWithValue("@Id", personId);
//                     cmd.Parameters.AddWithValue("@Type", "Individual");
//                     cmd.ExecuteNonQuery();
//                 }

//                 tx.Commit();
//             }
//             catch
//             {
//                 tx.Rollback();
//                 ViewBag.RegisterError = "Something went wrong while registering. Please try again.";
//                 return View("Register", page);
//             }

//             TempData["LoginMessage"] = "Account created. You can log in now.";
//             return RedirectToAction("Login");
//         }

//         [HttpPost, ValidateAntiForgeryToken]
//         public IActionResult RegisterFoundation([Bind(Prefix = "Foundation")] RegisterFoundationViewModel model)
//         {
//             var page = new RegisterPageViewModel { ActiveTab = "foundation", Foundation = model };

//             var domain = (model.EmailDomain ?? "").Trim().ToLowerInvariant();
//             var email = (model.ManagerEmail ?? "").Trim().ToLowerInvariant();

//             if (FoundationDomainExists(domain))
//                 ModelState.AddModelError("Foundation.EmailDomain", "That domain is already registered to a foundation.");
//             if (UsernameExists(email))
//                 ModelState.AddModelError("Foundation.ManagerEmail", "An account with this email already exists.");

//             if (!ModelState.IsValid) return View("Register", page);

//             using var connection = new SqlConnection(_connectionString);
//             connection.Open();
//             using var tx = connection.BeginTransaction();
//             try
//             {
//                 int foundationId;
//                 using (var cmd = new SqlCommand(@"
//                     INSERT INTO FOUNDATIONS (Name, EmailDomain, RegistrationNumber)
//                     OUTPUT INSERTED.Foundation_ID
//                     VALUES (@Name, @Domain, @Reg)", connection, tx))
//                 {
//                     cmd.Parameters.AddWithValue("@Name", model.FoundationName.Trim());
//                     cmd.Parameters.AddWithValue("@Domain", domain);
//                     cmd.Parameters.AddWithValue("@Reg", (object?)model.RegistrationNumber ?? DBNull.Value);
//                     foundationId = (int)cmd.ExecuteScalar()!;
//                 }

//                 var (first, last) = SplitName(model.ManagerName);
//                 int personId = InsertPerson(connection, tx, first, last, email, null, model.Password, "Manager", false, foundationId);

//                 using (var cmd = new SqlCommand(
//                     "INSERT INTO STAFF (Person_ID, Position) VALUES (@Id, 'Manager')", connection, tx))
//                 {
//                     cmd.Parameters.AddWithValue("@Id", personId);
//                     cmd.ExecuteNonQuery();
//                 }

//                 tx.Commit();
//             }
//             catch
//             {
//                 tx.Rollback();
//                 ViewBag.RegisterError = "Something went wrong while registering. Please try again.";
//                 return View("Register", page);
//             }

//             TempData["LoginMessage"] = "Foundation registered. Log in with your email to manage your staff.";
//             return RedirectToAction("Login");
//         }

//         // =====================================================================
//         // MANAGER: STAFF ACCOUNTS
//         // =====================================================================
//         [Authorize(Roles = "Manager")]
//         [HttpGet]
//         public IActionResult Staff() => BuildStaffPage();

//         [Authorize(Roles = "Manager")]
//         [HttpPost, ValidateAntiForgeryToken]
//         public IActionResult AddStaff([Bind(Prefix = "NewStaff")] AddStaffViewModel model)
//         {
//             var f = GetFoundation(CurrentFoundationId);
//             if (f == null) return Forbid();

//             var username = $"{(model.UsernameLocalPart ?? "").Trim().ToLowerInvariant()}@{f.Value.Domain}";
//             if (UsernameExists(username))
//                 ModelState.AddModelError("NewStaff.UsernameLocalPart", "That username is already taken.");

//             if (!ModelState.IsValid) return BuildStaffPage(model);

//             var temp = GenerateTempPassword();
//             var (first, last) = SplitName(model.FullName);

//             using var connection = new SqlConnection(_connectionString);
//             connection.Open();
//             using var tx = connection.BeginTransaction();
//             try
//             {
//                 int personId = InsertPerson(connection, tx, first, last, username, null, temp, "Staff", true, f.Value.Id);
//                 using var cmd = new SqlCommand(
//                     "INSERT INTO STAFF (Person_ID, Position) VALUES (@Id, 'Staff')", connection, tx);
//                 cmd.Parameters.AddWithValue("@Id", personId);
//                 cmd.ExecuteNonQuery();
//                 tx.Commit();
//             }
//             catch
//             {
//                 tx.Rollback();
//                 ViewBag.StaffError = "Could not create the account. Please try again.";
//                 return BuildStaffPage(model);
//             }

//             // Shown once on this response only; never stored in plain text.
//             return BuildStaffPage(null, username, temp);
//         }

//         [Authorize(Roles = "Manager")]
//         [HttpPost, ValidateAntiForgeryToken]
//         public IActionResult ResetStaffPassword(int id)
//         {
//             var staff = GetStaff(CurrentFoundationId).FirstOrDefault(s => s.Id == id);
//             if (staff == null) return NotFound();

//             var temp = GenerateTempPassword();
//             using var connection = new SqlConnection(_connectionString);
//             connection.Open();
//             using var cmd = new SqlCommand(@"
//                 UPDATE PERSONS SET PasswordHash = @Hash, MustChangePassword = 1
//                 WHERE Person_ID = @Id AND FoundationID = @F AND Role = 'Staff'", connection);
//             cmd.Parameters.AddWithValue("@Hash", PasswordHasher.Hash(temp));
//             cmd.Parameters.AddWithValue("@Id", id);
//             cmd.Parameters.AddWithValue("@F", CurrentFoundationId);
//             cmd.ExecuteNonQuery();

//             return BuildStaffPage(null, staff.Username, temp);
//         }

//         // =====================================================================
//         // CHANGE PASSWORD (any signed-in user)
//         // =====================================================================
//         [Authorize]
//         [HttpGet]
//         public IActionResult ChangePassword(bool first = false)
//         {
//             ViewBag.FirstLogin = first;
//             return View(new ChangePasswordViewModel());
//         }

//         [Authorize]
//         [HttpPost, ValidateAntiForgeryToken]
//         public IActionResult ChangePassword(ChangePasswordViewModel model)
//         {
//             if (!ModelState.IsValid) return View(model);

//             int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

//             using var connection = new SqlConnection(_connectionString);
//             connection.Open();

//             string? currentHash = null;
//             using (var cmd = new SqlCommand("SELECT PasswordHash FROM PERSONS WHERE Person_ID = @Id", connection))
//             {
//                 cmd.Parameters.AddWithValue("@Id", userId);
//                 currentHash = cmd.ExecuteScalar() as string;
//             }

//             if (currentHash == null || !PasswordHasher.Verify(model.CurrentPassword, currentHash))
//             {
//                 ViewBag.PasswordError = "Your current password is incorrect.";
//                 return View(model);
//             }

//             using (var cmd = new SqlCommand(
//                 "UPDATE PERSONS SET PasswordHash = @Hash, MustChangePassword = 0 WHERE Person_ID = @Id", connection))
//             {
//                 cmd.Parameters.AddWithValue("@Hash", PasswordHasher.Hash(model.NewPassword));
//                 cmd.Parameters.AddWithValue("@Id", userId);
//                 cmd.ExecuteNonQuery();
//             }

//             TempData["PasswordMessage"] = "Password updated.";
//             return RedirectByRole(User.FindFirstValue(ClaimTypes.Role) ?? "Staff");
//         }

//         // =====================================================================
//         // HELPERS
//         // =====================================================================

//         // Donors go to the donor portal, everyone else to the staff dashboard.
//         private IActionResult RedirectByRole(string role)
//             => role == "Donor"
//                 ? RedirectToAction("Index", "Donor")
//                 : RedirectToAction("Index", "Home");

//         // 0 if the claim is missing (GetFoundation(0) returns null, so callers Forbid).
//         private int CurrentFoundationId
//             => int.TryParse(User.FindFirstValue("FoundationId"), out var id) ? id : 0;

//         private IActionResult BuildStaffPage(AddStaffViewModel? form = null, string? newUser = null, string? tempPw = null)
//         {
//             var f = GetFoundation(CurrentFoundationId);
//             if (f == null) return Forbid();

//             return View("Staff", new StaffPageViewModel
//             {
//                 FoundationName = f.Value.Name,
//                 EmailDomain = f.Value.Domain,
//                 NewStaff = form ?? new AddStaffViewModel(),
//                 Staff = GetStaff(f.Value.Id),
//                 NewUsername = newUser,
//                 TempPassword = tempPw
//             });
//         }

//         private bool UsernameExists(string username)
//         {
//             using var connection = new SqlConnection(_connectionString);
//             connection.Open();
//             using var cmd = new SqlCommand("SELECT COUNT(1) FROM PERSONS WHERE Username = @U", connection);
//             cmd.Parameters.AddWithValue("@U", username);
//             return (int)cmd.ExecuteScalar()! > 0;
//         }

//         private bool FoundationDomainExists(string domain)
//         {
//             using var connection = new SqlConnection(_connectionString);
//             connection.Open();
//             using var cmd = new SqlCommand("SELECT COUNT(1) FROM FOUNDATIONS WHERE EmailDomain = @D", connection);
//             cmd.Parameters.AddWithValue("@D", domain);
//             return (int)cmd.ExecuteScalar()! > 0;
//         }

//         private (int Id, string Name, string Domain)? GetFoundation(int id)
//         {
//             using var connection = new SqlConnection(_connectionString);
//             connection.Open();
//             using var cmd = new SqlCommand("SELECT Foundation_ID, Name, EmailDomain FROM FOUNDATIONS WHERE Foundation_ID = @Id", connection);
//             cmd.Parameters.AddWithValue("@Id", id);
//             using var r = cmd.ExecuteReader();
//             return r.Read() ? (r.GetInt32(0), SafeString(r, 1), SafeString(r, 2)) : null;
//         }

//         private List<StaffRowViewModel> GetStaff(int foundationId)
//         {
//             var list = new List<StaffRowViewModel>();
//             using var connection = new SqlConnection(_connectionString);
//             connection.Open();
//             using var cmd = new SqlCommand(@"
//                 SELECT Person_ID, FName, LName, Username, MustChangePassword
//                 FROM PERSONS
//                 WHERE FoundationID = @F AND Role = 'Staff'
//                 ORDER BY FName, LName", connection);
//             cmd.Parameters.AddWithValue("@F", foundationId);
//             using var r = cmd.ExecuteReader();
//             while (r.Read())
//             {
//                 list.Add(new StaffRowViewModel
//                 {
//                     Id = r.GetInt32(0),
//                     FullName = $"{SafeString(r, 1)} {SafeString(r, 2)}".Trim(),
//                     Username = SafeString(r, 3),
//                     MustChangePassword = !r.IsDBNull(4) && r.GetBoolean(4)
//                 });
//             }
//             return list;
//         }

//         // FName, LName, etc. allow NULL in the database, so never call GetString directly.
//         private static string SafeString(SqlDataReader r, int index)
//             => r.IsDBNull(index) ? "" : r.GetString(index);

//         private static int InsertPerson(SqlConnection connection, SqlTransaction? tx,
//             string first, string last, string username, string? phone, string password,
//             string role, bool mustChange, int? foundationId = null)
//         {
//             using var cmd = new SqlCommand(@"
//                 INSERT INTO PERSONS (FName, LName, Username, PasswordHash, Phone, Role, FoundationID, MustChangePassword)
//                 OUTPUT INSERTED.Person_ID
//                 VALUES (@F, @L, @U, @H, @P, @R, @Fo, @M)", connection, tx);
//             cmd.Parameters.AddWithValue("@F", first);
//             cmd.Parameters.AddWithValue("@L", last);
//             cmd.Parameters.AddWithValue("@U", username);
//             cmd.Parameters.AddWithValue("@H", PasswordHasher.Hash(password));
//             cmd.Parameters.AddWithValue("@P", (object?)phone ?? DBNull.Value);
//             cmd.Parameters.AddWithValue("@R", role);
//             cmd.Parameters.AddWithValue("@Fo", (object?)foundationId ?? DBNull.Value);
//             cmd.Parameters.AddWithValue("@M", mustChange);
//             return (int)cmd.ExecuteScalar()!;
//         }

//         private static (string First, string Last) SplitName(string? full)
//         {
//             var parts = (full ?? "").Trim().Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
//             return (parts.Length > 0 ? parts[0] : "", parts.Length > 1 ? parts[1] : "");
//         }

//         // 12 chars, no look-alike characters (0/O, 1/l/I)
//         private static string GenerateTempPassword()
//         {
//             const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz23456789";
//             return new string(Enumerable.Range(0, 12)
//                 .Select(_ => chars[RandomNumberGenerator.GetInt32(chars.Length)]).ToArray());
//         }
//     }
// }

using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using FoundationManagement.Models;

namespace FoundationManagement.Controllers
{
    // Works against the Foundation_Activities2 database:
    //   PERSONS (Name, Surname, Cell_Number, Email, Username, Password, Person_Type)
    //   MANAGERS / STAFF / VOLUNTEERS hold the foundation link, DONORS holds donor details,
    //   CHARITY_FOUNDATIONS holds the foundations.
    public class AccountController : Controller
    {
        private readonly string _connectionString;

        public AccountController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("FoundationDb")
                ?? throw new InvalidOperationException("Connection string 'FoundationDb' not found in appsettings.json.");
        }

        // =====================================================================
        // WELCOME
        // =====================================================================
        [HttpGet]
        public IActionResult Welcome() => View();

        // =====================================================================
        // LOGIN / LOGOUT
        // =====================================================================
        [HttpGet]
        public IActionResult Login()
        {
            if (TempData["LoginMessage"] is string msg) ViewBag.LoginMessage = msg;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.LoginError = "Please enter your username and password.";
                return View(model);
            }

            // The foundation comes from whichever role table the person is in.
            const string sql = @"
                SELECT p.Person_ID, p.Name, p.Surname, p.Password, p.Person_Type,
                       COALESCE(m.Foundation_ID, s.Foundation_ID, v.Foundation_ID) AS Foundation_ID,
                       s.Position
                FROM PERSONS p
                LEFT JOIN MANAGERS   m ON m.Person_ID = p.Person_ID
                LEFT JOIN STAFF      s ON s.Person_ID = p.Person_ID
                LEFT JOIN VOLUNTEERS v ON v.Person_ID = p.Person_ID
                WHERE p.Username = @Username";

            var username = model.Username!.Trim().ToLowerInvariant();

            string? storedPassword = null;
            string? role = null;
            string fName = "", lName = "", position = "";
            int personId = 0;
            int? foundationId = null;

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using var command = new SqlCommand(sql, connection);
                command.Parameters.AddWithValue("@Username", username);

                using var reader = command.ExecuteReader();
                if (reader.Read())
                {
                    personId = reader.GetInt32(reader.GetOrdinal("Person_ID"));
                    fName = SafeString(reader, reader.GetOrdinal("Name"));
                    lName = SafeString(reader, reader.GetOrdinal("Surname"));
                    position = SafeString(reader, reader.GetOrdinal("Position"));

                    int po = reader.GetOrdinal("Password");
                    storedPassword = reader.IsDBNull(po) ? null : reader.GetString(po);

                    int ro = reader.GetOrdinal("Person_Type");
                    role = reader.IsDBNull(ro) ? null : reader.GetString(ro);

                    int fo = reader.GetOrdinal("Foundation_ID");
                    if (!reader.IsDBNull(fo)) foundationId = reader.GetInt32(fo);
                }
            }

            // Same message whether the username is unknown or the password is wrong.
            // An account with no password or no person type cannot log in either.
            if (storedPassword == null || role == null || !PasswordHasher.Verify(model.Password!, storedPassword))
            {
                ViewBag.LoginError = "Invalid username or password.";
                return View(model);
            }

            if (string.IsNullOrWhiteSpace(position)) position = role;

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.NameIdentifier, personId.ToString()),
                new Claim(ClaimTypes.Role, role),
                new Claim("FullName", $"{fName} {lName}".Trim()),
                new Claim("Position", position),
                new Claim("PersonId", personId.ToString())
            };
            if (foundationId.HasValue)
                claims.Add(new Claim("FoundationId", foundationId.Value.ToString()));

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

            return RedirectByRole(role);
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Welcome");
        }

        // =====================================================================
        // REGISTER
        // =====================================================================
        [HttpGet]
        public IActionResult Register(string tab = "donor")
            => View(new RegisterPageViewModel { ActiveTab = tab });

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult RegisterDonor([Bind(Prefix = "Donor")] RegisterDonorViewModel model)
        {
            var page = new RegisterPageViewModel { ActiveTab = "donor", Donor = model };

            // Donors log in with their email address, so it is stored as the username too.
            var email = (model.Email ?? "").Trim().ToLowerInvariant();
            if (PersonExists(email))
                ModelState.AddModelError("Donor.Email", "An account with this email already exists.");

            if (!ModelState.IsValid) return View("Register", page);

            var (first, last) = SplitName(model.FullName);

            using var connection = new SqlConnection(_connectionString);
            connection.Open();
            using var tx = connection.BeginTransaction();
            try
            {
                // 1. The person (login account)
                int personId = InsertPerson(connection, tx, first, last, CleanPhone(model.Phone),
                                            email, email, model.Password, "Donor");

                // 2. The donor record, linked to the person
                using var cmd = new SqlCommand(
                    "INSERT INTO DONORS (Person_ID, Donor_Type) VALUES (@Id, 'Individual')", connection, tx);
                cmd.Parameters.AddWithValue("@Id", personId);
                cmd.ExecuteNonQuery();

                tx.Commit();
            }
            catch
            {
                tx.Rollback();
                ViewBag.RegisterError = "Something went wrong while registering. Please try again.";
                return View("Register", page);
            }

            TempData["LoginMessage"] = "Account created. You can log in now.";
            return RedirectToAction("Login");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult RegisterFoundation([Bind(Prefix = "Foundation")] RegisterFoundationViewModel model)
        {
            var page = new RegisterPageViewModel { ActiveTab = "foundation", Foundation = model };

            var email = (model.ManagerEmail ?? "").Trim().ToLowerInvariant();
            if (PersonExists(email))
                ModelState.AddModelError("Foundation.ManagerEmail", "An account with this email already exists.");

            if (!ModelState.IsValid) return View("Register", page);

            using var connection = new SqlConnection(_connectionString);
            connection.Open();
            using var tx = connection.BeginTransaction();
            try
            {
                // 1. The foundation
                int foundationId;
                using (var cmd = new SqlCommand(@"
                    INSERT INTO CHARITY_FOUNDATIONS (F_Name)
                    OUTPUT INSERTED.Foundation_ID
                    VALUES (@Name)", connection, tx))
                {
                    cmd.Parameters.AddWithValue("@Name", model.FoundationName.Trim());
                    foundationId = (int)cmd.ExecuteScalar()!;
                }

                // 2. The manager's login account
                var (first, last) = SplitName(model.ManagerName);
                int personId = InsertPerson(connection, tx, first, last, null,
                                            email, email, model.Password, "Manager");

                // 3. The manager record, linking the person to the foundation
                using (var cmd = new SqlCommand(
                    "INSERT INTO MANAGERS (Person_ID, Foundation_ID) VALUES (@P, @F)", connection, tx))
                {
                    cmd.Parameters.AddWithValue("@P", personId);
                    cmd.Parameters.AddWithValue("@F", foundationId);
                    cmd.ExecuteNonQuery();
                }

                tx.Commit();
            }
            catch
            {
                tx.Rollback();
                ViewBag.RegisterError = "Something went wrong while registering. Please try again.";
                return View("Register", page);
            }

            TempData["LoginMessage"] = "Foundation registered. Log in with your email to manage your staff.";
            return RedirectToAction("Login");
        }

        // =====================================================================
        // MANAGER: STAFF ACCOUNTS
        // =====================================================================
        [Authorize(Roles = "Manager")]
        [HttpGet]
        public IActionResult Staff() => BuildStaffPage();

        [Authorize(Roles = "Manager")]
        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult AddStaff([Bind(Prefix = "NewStaff")] AddStaffViewModel model)
        {
            var f = GetFoundation(CurrentFoundationId);
            if (f == null) return Forbid();

            // The manager types the username; usernames are unique across the whole system.
            var username = (model.UsernameLocalPart ?? "").Trim().ToLowerInvariant();
            var email = (model.Email ?? "").Trim().ToLowerInvariant();

            if (PersonExists(username))
                ModelState.AddModelError("NewStaff.UsernameLocalPart", "That username is already taken.");
            if (PersonExists(email))
                ModelState.AddModelError("NewStaff.Email", "An account with this email already exists.");

            if (!ModelState.IsValid) return BuildStaffPage(model);

            var temp = GenerateTempPassword();
            var (first, last) = SplitName(model.FullName);

            using var connection = new SqlConnection(_connectionString);
            connection.Open();
            using var tx = connection.BeginTransaction();
            try
            {
                int personId = InsertPerson(connection, tx, first, last, null, email, username, temp, "Staff");

                using var cmd = new SqlCommand(
                    "INSERT INTO STAFF (Person_ID, Foundation_ID, Position) VALUES (@P, @F, 'Staff')", connection, tx);
                cmd.Parameters.AddWithValue("@P", personId);
                cmd.Parameters.AddWithValue("@F", f.Value.Id);
                cmd.ExecuteNonQuery();

                tx.Commit();
            }
            catch
            {
                tx.Rollback();
                ViewBag.StaffError = "Could not create the account. Please try again.";
                return BuildStaffPage(model);
            }

            // Shown once on this response only.
            return BuildStaffPage(null, username, temp);
        }

        [Authorize(Roles = "Manager")]
        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult ResetStaffPassword(int id)
        {
            var staff = GetStaff(CurrentFoundationId).FirstOrDefault(s => s.Id == id);
            if (staff == null) return NotFound();

            var temp = GenerateTempPassword();
            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            // Only staff who belong to this manager's foundation can be reset.
            using var cmd = new SqlCommand(@"
                UPDATE p SET p.Password = @Hash
                FROM PERSONS p
                JOIN STAFF s ON s.Person_ID = p.Person_ID
                WHERE p.Person_ID = @Id AND s.Foundation_ID = @F", connection);
            cmd.Parameters.AddWithValue("@Hash", PasswordHasher.Hash(temp));
            cmd.Parameters.AddWithValue("@Id", id);
            cmd.Parameters.AddWithValue("@F", CurrentFoundationId);
            cmd.ExecuteNonQuery();

            return BuildStaffPage(null, staff.Username, temp);
        }

        // =====================================================================
        // CHANGE PASSWORD (any signed-in user)
        // =====================================================================
        [Authorize]
        [HttpGet]
        public IActionResult ChangePassword(bool first = false)
        {
            ViewBag.FirstLogin = first;
            return View(new ChangePasswordViewModel());
        }

        [Authorize]
        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            string? currentPassword = null;
            using (var cmd = new SqlCommand("SELECT Password FROM PERSONS WHERE Person_ID = @Id", connection))
            {
                cmd.Parameters.AddWithValue("@Id", userId);
                currentPassword = cmd.ExecuteScalar() as string;
            }

            if (currentPassword == null || !PasswordHasher.Verify(model.CurrentPassword, currentPassword))
            {
                ViewBag.PasswordError = "Your current password is incorrect.";
                return View(model);
            }

            using (var cmd = new SqlCommand("UPDATE PERSONS SET Password = @Hash WHERE Person_ID = @Id", connection))
            {
                cmd.Parameters.AddWithValue("@Hash", PasswordHasher.Hash(model.NewPassword));
                cmd.Parameters.AddWithValue("@Id", userId);
                cmd.ExecuteNonQuery();
            }

            TempData["PasswordMessage"] = "Password updated.";
            return RedirectByRole(User.FindFirstValue(ClaimTypes.Role) ?? "Staff");
        }

        // =====================================================================
        // HELPERS
        // =====================================================================

        // Donors go to the donor portal, everyone else to the staff dashboard.
        private IActionResult RedirectByRole(string role)
            => role == "Donor"
                ? RedirectToAction("Index", "Donor")
                : RedirectToAction("Index", "Home");

        // 0 if the claim is missing (GetFoundation(0) returns null, so callers Forbid).
        private int CurrentFoundationId
            => int.TryParse(User.FindFirstValue("FoundationId"), out var id) ? id : 0;

        private IActionResult BuildStaffPage(AddStaffViewModel? form = null, string? newUser = null, string? tempPw = null)
        {
            var f = GetFoundation(CurrentFoundationId);
            if (f == null) return Forbid();

            return View("Staff", new StaffPageViewModel
            {
                FoundationName = f.Value.Name,
                NewStaff = form ?? new AddStaffViewModel(),
                Staff = GetStaff(f.Value.Id),
                NewUsername = newUser,
                TempPassword = tempPw
            });
        }

        // True if the value is already used as a username or as an email.
        private bool PersonExists(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return false;

            using var connection = new SqlConnection(_connectionString);
            connection.Open();
            using var cmd = new SqlCommand(
                "SELECT COUNT(1) FROM PERSONS WHERE Username = @V OR Email = @V", connection);
            cmd.Parameters.AddWithValue("@V", value);
            return (int)cmd.ExecuteScalar()! > 0;
        }

        private (int Id, string Name)? GetFoundation(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();
            using var cmd = new SqlCommand(
                "SELECT Foundation_ID, F_Name FROM CHARITY_FOUNDATIONS WHERE Foundation_ID = @Id", connection);
            cmd.Parameters.AddWithValue("@Id", id);
            using var r = cmd.ExecuteReader();
            return r.Read() ? (r.GetInt32(0), SafeString(r, 1)) : null;
        }

        private List<StaffRowViewModel> GetStaff(int foundationId)
        {
            var list = new List<StaffRowViewModel>();
            using var connection = new SqlConnection(_connectionString);
            connection.Open();
            using var cmd = new SqlCommand(@"
                SELECT p.Person_ID, p.Name, p.Surname, p.Username
                FROM STAFF s
                JOIN PERSONS p ON p.Person_ID = s.Person_ID
                WHERE s.Foundation_ID = @F
                ORDER BY p.Name, p.Surname", connection);
            cmd.Parameters.AddWithValue("@F", foundationId);
            using var r = cmd.ExecuteReader();
            while (r.Read())
            {
                list.Add(new StaffRowViewModel
                {
                    Id = r.GetInt32(0),
                    FullName = $"{SafeString(r, 1)} {SafeString(r, 2)}".Trim(),
                    Username = SafeString(r, 3)
                });
            }
            return list;
        }

        // Name, Surname and Position can be NULL in the database, so never call GetString directly.
        private static string SafeString(SqlDataReader r, int index)
            => r.IsDBNull(index) ? "" : r.GetString(index);

        private static int InsertPerson(SqlConnection connection, SqlTransaction tx,
            string name, string surname, string? cell, string email, string username,
            string password, string personType)
        {
            using var cmd = new SqlCommand(@"
                INSERT INTO PERSONS (Name, Surname, Cell_Number, Email, Username, Password, Person_Type)
                OUTPUT INSERTED.Person_ID
                VALUES (@N, @S, @C, @E, @U, @P, @T)", connection, tx);
            cmd.Parameters.AddWithValue("@N", name);
            cmd.Parameters.AddWithValue("@S", surname);
            cmd.Parameters.AddWithValue("@C", (object?)cell ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@E", email);
            cmd.Parameters.AddWithValue("@U", username);
            cmd.Parameters.AddWithValue("@P", PasswordHasher.Hash(password));
            cmd.Parameters.AddWithValue("@T", personType);
            return (int)cmd.ExecuteScalar()!;
        }

        private static (string First, string Last) SplitName(string? full)
        {
            var parts = (full ?? "").Trim().Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
            return (parts.Length > 0 ? parts[0] : "", parts.Length > 1 ? parts[1] : "");
        }

        // Cell_Number only allows digits, spaces and a plus sign, and holds 15 characters.
        // Keep digits and "+", drop brackets and dashes, and store nothing if it is still too long.
        private static string? CleanPhone(string? phone)
        {
            if (string.IsNullOrWhiteSpace(phone)) return null;
            var cleaned = new string(phone.Where(c => char.IsDigit(c) || c == '+').ToArray());
            return cleaned.Length is > 0 and <= 15 ? cleaned : null;
        }

        // 12 chars, no look-alike characters (0/O, 1/l/I)
        private static string GenerateTempPassword()
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz23456789";
            return new string(Enumerable.Range(0, 12)
                .Select(_ => chars[RandomNumberGenerator.GetInt32(chars.Length)]).ToArray());
        }
    }
}