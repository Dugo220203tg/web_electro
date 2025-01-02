using System;
using System.Collections.Generic;

namespace API_Web_Shop_Electronic_TD.Data;

public partial class PayHistory
{
    public int Id { get; set; }

    public string? FullName { get; set; }

    public string? OrderInfo { get; set; }

    public double? Amount { get; set; }

    public string? PayMethod { get; set; }

    public string? CouponCode { get; set; }

    public DateTime? CreateDate { get; set; }
}
