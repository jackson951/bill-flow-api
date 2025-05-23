﻿using System;
using System.Collections.Generic;

namespace BillFlow.API.Models;

public partial class User
{
    public Guid Id { get; set; }

    public string FullName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string CompanyName { get; set; } = null!;

    public string Role { get; set; } = null!;

    public bool Verified { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();
}
