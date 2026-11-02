using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using FoundationManagement.Models;

namespace FoundationManagement.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly string _connectionString;

    public HomeController(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("FoundationDb")
            ?? throw new InvalidOperationException("Connection string 'FoundationDb' not found in appsettings.json.");
    }

    public IActionResult Index()
    {
        return View();
    }

    //  FOOD DONATIONS

    // GET: shows the empty form, with the donor dropdown populated
    public IActionResult FoodDonations()
    {
        ViewBag.Donors = GetDonors();
        return View();
    }

    // POST: receives the submitted form and saves it across both tables
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult FoodDonations(FoodDonationFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            // Reload the donor list before showing the form again, since
            // dropdown options aren't preserved automatically on a failed post.
            ViewBag.Donors = GetDonors();
            return View(model);
        }

        using var connection = new SqlConnection(_connectionString);
        connection.Open();

        // Step 1: insert the mother row (DONATIONS)
        // OUTPUT INSERTED.Donation_ID hands back the auto-generated identity
        // value straight from the INSERT, so we can use it as the foreign key below.
        string insertDonationSql = @"
            INSERT INTO DONATIONS (Donor_ID, Donation_Date, Recorded_By, Created_At, Item_Count)
            OUTPUT INSERTED.Donation_ID
            VALUES (@DonorId, @DonationDate, @RecordedBy, @CreatedAt, @ItemCount)";

        int newDonationId;

        using (var command = new SqlCommand(insertDonationSql, connection))
        {
            command.Parameters.AddWithValue("@DonorId", (object?)model.DonorId ?? DBNull.Value);
            command.Parameters.AddWithValue("@DonationDate", (object?)model.DonationDate ?? DBNull.Value);
            command.Parameters.AddWithValue("@RecordedBy", "AMY"); // TODO: replace with the signed-in staff member once auth is wired up
            command.Parameters.AddWithValue("@CreatedAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm"));
            command.Parameters.AddWithValue("@ItemCount", (object?)model.ItemCount ?? DBNull.Value);

            newDonationId = (int)command.ExecuteScalar()!;
        }

        // Step 2: insert the child row (FOOD_DONATIONS)
        string insertFoodDonationSql = @"
            INSERT INTO FOOD_DONATIONS (Donation_ID, Food_Type, Item_Weight, Unit_of_Measure, ExpiryDate)
            VALUES (@DonationId, @FoodType, @ItemWeight, @UnitOfMeasure, @ExpiryDate)";

        using (var command = new SqlCommand(insertFoodDonationSql, connection))
        {
            command.Parameters.AddWithValue("@DonationId", newDonationId);
            command.Parameters.AddWithValue("@FoodType", (object?)model.FoodType ?? DBNull.Value);
            command.Parameters.AddWithValue("@ItemWeight", (object?)model.ItemWeight ?? DBNull.Value);
            command.Parameters.AddWithValue("@UnitOfMeasure", (object?)model.UnitOfMeasure ?? DBNull.Value);
            command.Parameters.AddWithValue("@ExpiryDate", (object?)model.ExpiryDate ?? DBNull.Value);

            command.ExecuteNonQuery();
        }

        TempData["SuccessMessage"] = "Food donation recorded successfully.";
        return RedirectToAction("FoodDonations");
    }

    // CLOTHING DONATIONS

    // GET: shows the empty clothing form, with the donor dropdown populated
    public IActionResult ClothingDonations()
    {
        ViewBag.Donors = GetDonors();
        return View();
    }

    // POST: receives the submitted clothing form and saves it across both tables
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ClothingDonations(ClothingDonationFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Donors = GetDonors();
            return View(model);
        }

        using var connection = new SqlConnection(_connectionString);
        connection.Open();

        // Step 1: insert the mother row (DONATIONS)
        string insertDonationSql = @"
            INSERT INTO DONATIONS (Donor_ID, Donation_Date, Recorded_By, Created_At, Item_Count)
            OUTPUT INSERTED.Donation_ID
            VALUES (@DonorId, @DonationDate, @RecordedBy, @CreatedAt, @ItemCount)";

        int newDonationId;

        using (var command = new SqlCommand(insertDonationSql, connection))
        {
            command.Parameters.AddWithValue("@DonorId", (object?)model.DonorId ?? DBNull.Value);
            command.Parameters.AddWithValue("@DonationDate", (object?)model.DonationDate ?? DBNull.Value);
            command.Parameters.AddWithValue("@RecordedBy", "AMY"); // TODO: replace with the signed-in staff member once auth is wired up
            command.Parameters.AddWithValue("@CreatedAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm"));
            command.Parameters.AddWithValue("@ItemCount", (object?)model.ItemCount ?? DBNull.Value);

            newDonationId = (int)command.ExecuteScalar()!;
        }

        // Step 2: insert the child row (CLOTHING_DONATIONS)
        string insertClothingDonationSql = @"
            INSERT INTO CLOTHING_DONATIONS (Donation_ID, Clothing_Type, Size_Category, Condition_Status)
            VALUES (@DonationId, @ClothingType, @SizeCategory, @ConditionStatus)";

        using (var command = new SqlCommand(insertClothingDonationSql, connection))
        {
            command.Parameters.AddWithValue("@DonationId", newDonationId);
            command.Parameters.AddWithValue("@ClothingType", (object?)model.ClothingType ?? DBNull.Value);
            command.Parameters.AddWithValue("@SizeCategory", (object?)model.SizeCategory ?? DBNull.Value);
            command.Parameters.AddWithValue("@ConditionStatus", (object?)model.ConditionStatus ?? DBNull.Value);

            command.ExecuteNonQuery();
        }

        TempData["SuccessMessage"] = "Clothing donation recorded successfully.";
        return RedirectToAction("ClothingDonations");
    }

    // FOOD STOCK (detailed report)

    public IActionResult FoodStock()
    {
        var rows = new List<FoodDonationDetailRow>();

        string sql = @"
            SELECT fd.FD_ID, fd.Food_Type, fd.Item_Weight, fd.Unit_of_Measure, fd.ExpiryDate,
                   d.Donation_Date, d.Recorded_By
            FROM FOOD_DONATIONS fd
            JOIN DONATIONS d ON fd.Donation_ID = d.Donation_ID
            ORDER BY d.Donation_Date DESC";

        using (var connection = new SqlConnection(_connectionString))
        {
            connection.Open();

            using var command = new SqlCommand(sql, connection);
            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                rows.Add(new FoodDonationDetailRow
                {
                    FdId = reader.GetInt32(reader.GetOrdinal("FD_ID")),
                    FoodType = reader["Food_Type"] as string,
                    ItemWeight = reader["Item_Weight"] as string,
                    UnitOfMeasure = reader["Unit_of_Measure"] as string,
                    ExpiryDate = reader["ExpiryDate"] as string,
                    DonationDate = reader["Donation_Date"] as string,
                    RecordedBy = reader["Recorded_By"] as string
                });
            }
        }
        return View(rows);
    }

    //Pledges
    //Getting the list of all pldeges
    public IActionResult Pledges(){
        var rows = new List<PledgeRow>();
        string sql = @"
        SELECT PledgeID, Donation_Type, Item_Name, Quantity, Unit_of_Measure,
                Item_Count, CurrentDate, ExpectedDate, Status, Recorded_By
                FROM DONOR_PLEDGES
                ORDER BY PledgeID DESC";      
        using var connection = new SqlConnection(_connectionString);
        connection.Open();
        using var command = new SqlCommand(sql, connection);
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            rows.Add(new PledgeRow
            {
                PledgeID = reader.GetInt32(reader.GetOrdinal("PledgeID")),
                DonationType = reader["Donation_Type"] as string?? "",
                ItemName = reader["Item_Name"] as string ??"",
                Quantity = reader["Quantity"] is DBNull ? 0 : Convert.ToInt32 (reader["Quantity"]),
                UnitOfMeasure = reader["Unit_of_Measure"] as string?? "",
                ItemCount = reader["Item_Count"] is DBNull ? 0 : Convert.ToInt32 (reader["Item_Count"]),
                CurrentDate = reader["CurrentDate"] as string ?? "",
                ExpectedDate = reader["ExpectedDate"] as string ?? "",
                Status = reader["Status"] as string ?? "",
                RecordedBy = reader["Recorded_By"] as string ?? ""
            });
        }
        return View("PledgeViewStaff", rows);  
    }
    //load one pledge into the status form
    [HttpGet]
    public IActionResult UpdatePledgeStatus (int id){
        string sql = @"
                SELECT PledgeID, Donation_Type, Item_Name, Quantity, Unit_of_Measure, Item_Count,
                CurrentDate, ExpecteDate, Status, Recorded_By
                FROM DONOR_PLEDGES
                WHERE PledgeID = @Id";

        using var connection = new SqlConnection (_connectionString);
        connection.Open();
        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@Id", id);
        using var reader = command.ExecuteReader();

        if (!reader.Read()) return NotFound();

        var model = new PledgeStatusFormViewModel{
                PledgeID = reader.GetInt32(reader.GetOrdinal("PledgeID")),
                DonationType = reader["Donation_Type"] as string?? "",
                ItemName = reader["Item_Name"] as string ??"",
                Quantity = reader["Quantity"] is DBNull ? 0: Convert.ToInt32 (reader["Quantity"]),
                UnitOfMeasure = reader["Unit_of_Measure"] as string?? "",
                ItemCount = reader["Item_Count"] is DBNull?0: Convert.ToInt32 (reader["Item_Count"]),
                CurrentDate = reader["CurrentDate"] as string ?? "",
                ExpectedDate = reader["ExpectedDate"] as string ?? "",
                Status = reader["Status"] as string ?? "",
                RecordedBy = reader["Recorded_By"] as string ?? ""
        };
        return View (model);
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult UpdatePledgeStatus (int pledgeId, string status)
    {
        var allowed = new[] {"Review", "Honoured", "Not Honoured", "Cancelled"};
        if (!allowed.Contains(status)) return BadRequest();

        string sql = "UPDATE DONOR_PLEDGES SET Status = @Status WHERE PledgeID = @Id";

        using var connection = new SqlConnection(_connectionString);
        connection.Open();
        using var command = new SqlCommand (sql, connection);
        command.Parameters.AddWithValue("@Status", status);
        command.Parameters.AddWithValue("@Id", pledgeId);
        command.ExecuteNonQuery();

        TempData["SuccessMessage"] = "Pledge status updated.";
        return RedirectToAction("Pledges");
    }

    // SHARED
    private List<DonorOption> GetDonors()
    {
        var donors = new List<DonorOption>();

        string sql = @"
            SELECT d.Donor_ID, p.FName, p.LName
            FROM DONOR d
            LEFT JOIN PERSONS p ON d.Person_ID = p.Person_ID";

        using var connection = new SqlConnection(_connectionString);
        connection.Open();

        using var command = new SqlCommand(sql, connection);
        using var reader = command.ExecuteReader();

        while (reader.Read())
        {
            bool hasName = !reader.IsDBNull(reader.GetOrdinal("FName"));

            donors.Add(new DonorOption
            {
                DonorId = reader.GetInt32(reader.GetOrdinal("Donor_ID")),
                Name = hasName
                    ? $"{reader["FName"]} {reader["LName"]}"
                    : "Unknown donor"
            });
        }

        return donors;
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}