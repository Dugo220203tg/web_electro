using System;
using System.Collections.Generic;

namespace TDProjectMVC.Data;

public partial class Coupon
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public string? Description { get; set; }

    public int? Quantity { get; set; }

    public decimal? Price { get; set; }

    public DateTime? DateStart { get; set; }

    public DateTime? DateEnd { get; set; }

    public int? Status { get; set; }
}
