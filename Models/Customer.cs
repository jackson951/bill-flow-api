using System;
using System.Collections.Generic;

namespace BillFlow.API.Models;

public partial class Customer
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Email { get; set; }

    public string? Phone { get; set; }

    public string? Address { get; set; }

    public string Type { get; set; } = null!;

    public Guid UserId { get; set; }
}
