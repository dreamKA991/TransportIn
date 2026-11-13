using System;
using System.Collections.Generic;

namespace TransportIn.Data.Entities;

public partial class ContactPerson
{
    public int Id { get; set; }

    public int CustomerId { get; set; }

    public string FullName { get; set; } = null!;

    public string Position { get; set; } = null!;

    public string Phone { get; set; } = null!;

    public string Email { get; set; } = null!;

    public bool IsPrimary { get; set; }

    public bool IsActive { get; set; }

    public virtual Customer Customer { get; set; } = null!;

    public virtual ICollection<OrderPoint> OrderPoints { get; set; } = new List<OrderPoint>();
}
