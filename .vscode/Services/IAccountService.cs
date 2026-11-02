using FoundationManagement.Models;

namespace FoundationManagement.Services
{
    /// <summary>
    /// The only part that touches your database. Implement it with whatever data access
    /// you are using now (ADO.NET / Dapper / etc.). Hash passwords with your PashwordHasher
    /// inside the implementation - the controller only ever passes plain text in.
    ///
    /// Suggested tables:
    ///   Foundations(Id, Name, EmailDomain UNIQUE, RegistrationNumber, CreatedAt)
    ///   Users(Id, FoundationId NULL, FullName, Username UNIQUE, PasswordHash,
    ///         Role  /* 'Donor' | 'Manager' | 'Staff' */, MustChangePassword BIT, CreatedAt)
    /// </summary>
    public interface IAccountService
    {
        bool UsernameExists(string username);
        bool FoundationDomainExists(string domain);

        void CreateDonor(RegisterDonorViewModel model);

        // Creates the foundation AND its manager (Role = Manager) in one transaction.
        // The manager's username is their email.
        void CreateFoundationWithManager(RegisterFoundationViewModel model);

        // Role = Staff, MustChangePassword = true
        void CreateStaff(int foundationId, string fullName, string username, string tempPassword);

        void ResetStaffPassword(int foundationId, int staffId, string tempPassword);

        List<StaffRowViewModel> GetStaff(int foundationId);

        // Returns (id, name, domain) or null
        (int Id, string Name, string Domain)? GetFoundation(int foundationId);

        // Verifies current password, then saves the new one and clears MustChangePassword.
        bool ChangePassword(int userId, string currentPassword, string newPassword);
    }
}