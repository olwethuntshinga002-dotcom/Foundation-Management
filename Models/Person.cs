using System;
using System.Collections.Generic;

namespace FoundationManagement.Models;

public partial class Person
{
    public int PersonId { get; set; }

    public string? Fname { get; set; }

    public string? Lname { get; set; }

    public string? Cellnum { get; set; }

    public string? Email { get; set; }

    public string? Gender { get; set; }

    public string? Username { get; set; }

    public virtual ICollection<Donor> Donors { get; set; } = new List<Donor>();
}
