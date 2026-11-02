namespace FoundationManagement.Models;

public class FoodDonationDetailRow
{
    public int FdId { get; set; }
    public string? FoodType { get; set; }
    public string? ItemWeight { get; set; }
    public string? UnitOfMeasure { get; set; }
    public string? ExpiryDate { get; set; }
    public string? DonationDate { get; set; }
    public string? RecordedBy { get; set; }
}