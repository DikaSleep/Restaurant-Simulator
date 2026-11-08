using System;
using System.Collections.Generic;

namespace Restaurant_Simulator.Models;

public partial class Category
{
    public int CategoryId { get; set; }

    public string CategoryName { get; set; } = null!;

    public string? Description { get; set; }

    public virtual ICollection<Menu> Menus { get; set; } = new List<Menu>();
}
