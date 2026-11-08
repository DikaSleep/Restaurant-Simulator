using System;
using System.Collections.Generic;

namespace Restaurant_Simulator.Models;

public partial class Order
{
    public int OrderId { get; set; }

    public int UserId { get; set; }

    public int? EmployeeId { get; set; }

    public int TableId { get; set; }

    public DateTime OrderDate { get; set; }

    public string Status { get; set; } = null!;

    public decimal TotalAmount { get; set; }

    public string? Notes { get; set; }

    public virtual Employee? Employee { get; set; }

    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

    public virtual Payment? Payment { get; set; }

    public virtual RestaurantTable Table { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
