using System;
using System.Collections.Generic;

namespace Restaurant_Simulator.Models;

public partial class Payment
{
    public int PaymentId { get; set; }

    public int OrderId { get; set; }

    public string PaymentMethod { get; set; } = null!;

    public decimal Amount { get; set; }

    public DateTime PaymentDate { get; set; }

    public string Status { get; set; } = null!;

    public string? ReferenceNumber { get; set; }

    public virtual Order Order { get; set; } = null!;
}
