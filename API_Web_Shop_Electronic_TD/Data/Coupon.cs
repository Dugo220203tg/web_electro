using System;
using System.Collections.Generic;

namespace API_Web_Shop_Electronic_TD.Data;

public partial class Coupon
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public int Quantility { get; set; }

    public DateTime? DateEnd { get; set; }

    public DateTime? DateStart { get; set; }

    public decimal Price { get; set; }

    public int Status { get; set; }
}
