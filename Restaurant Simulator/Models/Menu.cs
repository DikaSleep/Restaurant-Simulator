using System;
using System.Collections.Generic;

namespace Restaurant_Simulator.Models;

public partial class Menu
{
    public int MenuId { get; set; }

    public int CategoryId { get; set; }

    public string MenuName { get; set; } = null!;

    public string? Description { get; set; }

    public decimal Price { get; set; }

    public int Stock { get; set; }

    public string? ImageUrl { get; set; }

    public bool IsAvailable { get; set; }

    public virtual Category Category { get; set; } = null!;

    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
}
