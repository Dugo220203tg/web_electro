﻿using System;
using System.Collections.Generic;

namespace API_Web_Shop_Electronic_TD.Data;

public partial class Loai
{
    public int MaLoai { get; set; }

    public string TenLoai { get; set; } = null!;

    public string? TenLoaiAlias { get; set; }

    public string? MoTa { get; set; }

    public string? Hinh { get; set; }

    public int? DanhMucId { get; set; }

    public virtual DanhMucSp? DanhMuc { get; set; }

    public virtual ICollection<HangHoa> HangHoas { get; set; } = new List<HangHoa>();
}
