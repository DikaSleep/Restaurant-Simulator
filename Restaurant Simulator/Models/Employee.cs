using System;
using System.Collections.Generic;

namespace Restaurant_Simulator.Models;

public partial class Employee
{
    public int EmployeeId { get; set; }

    public string FullName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string? PhoneNumber { get; set; }

    public string Position { get; set; } = null!;

    public decimal Salary { get; set; }

    public DateOnly HireDate { get; set; }

    public bool IsActive { get; set; }

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
