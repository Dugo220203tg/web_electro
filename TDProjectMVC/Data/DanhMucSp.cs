using System;
using System.Collections.Generic;

namespace TDProjectMVC.Data;

public partial class DanhMucSp
{
    public int MaDanhMuc { get; set; }

    public string TenDanhMuc { get; set; } = null!;

    public virtual ICollection<Loai> Loais { get; set; } = new List<Loai>();
}
