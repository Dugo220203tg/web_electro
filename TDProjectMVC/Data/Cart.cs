using System;
using System.Collections.Generic;

namespace TDProjectMVC.Data;

public partial class Cart
{
    public int Id { get; set; }

    public string UserId { get; set; } = null!;

    public int ProductId { get; set; }

    public DateTime? CreateAt { get; set; }

    public int? Quantity { get; set; }

    public virtual HangHoa Product { get; set; } = null!;

    public virtual KhachHang User { get; set; } = null!;
}
