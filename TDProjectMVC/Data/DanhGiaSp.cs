using System;
using System.Collections.Generic;

namespace TDProjectMVC.Data;

public partial class DanhGiaSp
{
    public string MaKh { get; set; } = null!;

    public int? Sao { get; set; }

    public int MaHh { get; set; }

    public DateTime? Ngay { get; set; }

    public string NoiDung { get; set; } = null!;

    public int? TrangThai { get; set; }

    public int MaDg { get; set; }

    public virtual HangHoa MaHhNavigation { get; set; } = null!;

    public virtual KhachHang MaKhNavigation { get; set; } = null!;
}
