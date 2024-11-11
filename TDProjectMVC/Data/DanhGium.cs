using System;
using System.Collections.Generic;

namespace TDProjectMVC.Data;

public partial class DanhGium
{
    public int MaDg { get; set; }

    public string MaKh { get; set; } = null!;

    public string NoiDung { get; set; } = null!;

    public int? DanhGia { get; set; }

    public DateTime? Ngay { get; set; }

    public int? MaHh { get; set; }

    public virtual KhachHang MaKhNavigation { get; set; } = null!;
}
