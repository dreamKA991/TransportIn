using System;
using System.Collections.Generic;

namespace TransportIn.Data.Entities;

public partial class Customer
{
    public int Id { get; set; }

    public string CompanyName { get; set; } = null!;

    public string Edrpou { get; set; } = null!;

    public string Address { get; set; } = null!;

    public string Phone { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Notes { get; set; } = null!;

    public bool IsActive { get; set; }

    public virtual ICollection<ContactPerson> ContactPeople { get; set; } = new List<ContactPerson>();

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
