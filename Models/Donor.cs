using System;
using System.Collections.Generic;

namespace FoundationManagement.Models;

public partial class Donor
{
    public int DonorId { get; set; }

    public int? PersonId { get; set; }

    public string? DonorType { get; set; }

    public virtual ICollection<Donation> Donations { get; set; } = new List<Donation>();

    public virtual Person? Person { get; set; }
}
