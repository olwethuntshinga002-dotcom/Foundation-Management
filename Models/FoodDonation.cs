using System;
using System.Collections.Generic;

namespace FoundationManagement.Models;

public partial class FoodDonation
{
    public int FdId { get; set; }

    public int? DonationId { get; set; }

    public string? FoodType { get; set; }

    public string? ItemWeight { get; set; }

    public string? UnitOfMeasure { get; set; }

    public string? ExpiryDate { get; set; }

    public virtual Donation? Donation { get; set; }
}
