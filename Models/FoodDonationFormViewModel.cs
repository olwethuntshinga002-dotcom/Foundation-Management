using System.ComponentModel.DataAnnotations;

namespace FoundationManagement.Models;

public class FoodDonationFormViewModel
{
    // Donations (mother) 

    [Required(ErrorMessage = "Please select a donor.")]
    public int DonorId { get; set; }

    [Required(ErrorMessage = "Please enter a donation date.")]
    public string? DonationDate { get; set; }

    public int? ItemCount { get; set; }

    // Food Donations (child)

    [Required(ErrorMessage = "Please select a food type.")]
    public string? FoodType { get; set; }

    [Required(ErrorMessage = "Please enter a weight.")]
    public string? ItemWeight { get; set; }

    [Required(ErrorMessage = "Please select a unit of measure.")]
    public string? UnitOfMeasure { get; set; }

    public string? ExpiryDate { get; set; }
}