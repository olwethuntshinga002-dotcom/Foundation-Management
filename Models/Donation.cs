using System;
using System.Collections.Generic;

namespace FoundationManagement.Models;

public partial class Donation
{
    public int DonationId { get; set; }

    public int? DonorId { get; set; }

    public string? DonationDate { get; set; }

    public string? RecordedBy { get; set; }

    public string? CreatedAt { get; set; }

    public int? ItemCount { get; set; }

    public virtual Donor? Donor { get; set; }

    public virtual ICollection<FoodDonation> FoodDonations { get; set; } = new List<FoodDonation>();
}
