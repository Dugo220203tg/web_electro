using System.ComponentModel.DataAnnotations;

namespace TrangQuanLy.Models
{
    public class DanhMucViewModel
    {
        [Key]
        public int MaDanhMuc { get; set; }
        [Required(ErrorMessage = "Tên danh mục không được để trống")]
        public string TenDanhMuc { get; set; }
    }
}
