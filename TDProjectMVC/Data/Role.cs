using System;
using System.Collections.Generic;

namespace TDProjectMVC.Data;

public partial class Role
{
    public int Id { get; set; }

    public string? RoleName { get; set; }

    public string? Description { get; set; }
}
