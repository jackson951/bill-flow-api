using System;
using System.Collections.Generic;

namespace BillFlow.API.Models;

public partial class Employee
{
    public Guid Id { get; set; }

    public string FullName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string CompanyName { get; set; } = null!;

    public string Role { get; set; } = null!;

    public bool Verified { get; set; }

    public DateTime CreatedAt { get; set; }

    public Guid UserId { get; set; }

    public virtual User User { get; set; } = null!;
}
