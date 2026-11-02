using System;
namespace FoundationManagement.Models
{
    // One row in the Pledges view page
    public class PledgeRow
    {
        public int PledgeID { get; set; }
        public string DonationType { get; set; } = string.Empty;
        public string ItemName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public string UnitOfMeasure { get; set; } = string.Empty;
        public int ItemCount { get; set; }
        public string CurrentDate { get; set; } = string.Empty;
        public string ExpectedDate { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string RecordedBy { get; set; } = string.Empty;
    }
    //Update Pledge Status page.
    //PledgeID and Status are used, the rest is for display.
    public class PledgeStatusFormViewModel
    {
        public int PledgeID { get; set; }
        public string DonationType { get; set; } = string.Empty;
        public string ItemName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public string UnitOfMeasure { get; set; } = string.Empty;
        public int ItemCount { get; set; }
        public string CurrentDate { get; set; } = string.Empty;
        public string ExpectedDate { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string RecordedBy { get; set; } = string.Empty;
    }
}