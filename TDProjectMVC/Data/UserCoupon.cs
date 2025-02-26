﻿using System;
using System.Collections.Generic;

namespace TDProjectMVC.Data;

public partial class UserCoupon
{
    public string UserId { get; set; } = null!;

    public int CouponId { get; set; }

    public int? Quantity { get; set; }

    public virtual Coupon Coupon { get; set; } = null!;

    public virtual KhachHang User { get; set; } = null!;
}
