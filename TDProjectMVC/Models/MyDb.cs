using Microsoft.EntityFrameworkCore;
using TDProjectMVC.ViewModels;

namespace TDProjectMVC.Models
{
    public class MyDb : DbContext
    {
        public MyDb(DbContextOptions options) : base(options)
        {
        }
        public DbSet<RegisterVM> KhachHangs { get; set; }

    }
}
