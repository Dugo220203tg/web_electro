﻿using System;
using System.Collections.Generic;

namespace API_Web_Shop_Electronic_TD.Data;

public partial class DanhMucSp
{
    public int MaDanhMuc { get; set; }

    public string TenDanhMuc { get; set; } = null!;

    public string? Image { get; set; }

    public virtual ICollection<Loai> Loais { get; set; } = new List<Loai>();
}
