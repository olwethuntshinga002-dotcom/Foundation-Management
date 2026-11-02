using System.ComponentModel.DataAnnotations;

namespace FoundationManagement.Models;

public class ClothingDonationFormViewModel
{
    // Donations (mother)

    [Required(ErrorMessage = "Please select a donor.")]
    public int? DonorId { get; set; }

    [Required(ErrorMessage = "Please enter a donation date.")]
    public string? DonationDate { get; set; }

    public int? ItemCount { get; set; }

    // Clothing Donations (child)

    [Required(ErrorMessage = "Please select a clothing type.")]
    public string? ClothingType { get; set; }

    [Required(ErrorMessage = "Please select a size category.")]
    public string? SizeCategory { get; set; }

    [Required(ErrorMessage = "Please select a condition.")]
    public string? ConditionStatus { get; set; }
}