﻿using System;
using System.Collections.Generic;

namespace API_Web_Shop_Electronic_TD.Data;

public partial class PayHistory
{
    public int Id { get; set; }

    public string OrderId { get; set; } = null!;

    public string? FullName { get; set; }

    public string? OrderInfo { get; set; }

    public string? Amount { get; set; }

    public DateTime? CreateDate { get; set; }

    public string? PayMethod { get; set; }

    public string? CouponCode { get; set; }
}
