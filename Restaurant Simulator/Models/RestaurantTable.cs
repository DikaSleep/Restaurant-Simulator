using System;
using System.Collections.Generic;

namespace Restaurant_Simulator.Models;

public partial class RestaurantTable
{
    public int TableId { get; set; }

    public string TableNumber { get; set; } = null!;

    public int Capacity { get; set; }

    public string Status { get; set; } = null!;

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
