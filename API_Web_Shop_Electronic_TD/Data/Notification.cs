using System;
using System.Collections.Generic;

namespace API_Web_Shop_Electronic_TD.Data;

public partial class Notification
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public string? Description { get; set; }

    public bool? Status { get; set; }

    public DateTime? CreateAt { get; set; }
}
