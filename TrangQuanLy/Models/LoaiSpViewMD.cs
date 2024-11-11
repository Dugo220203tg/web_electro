using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace TrangQuanLy.Models
{
    public class LoaiSpViewMD
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MaLoai { get; set; }
        public string? TenLoai { get; set; }
        public string Hinh { get; set; }
        public string Mota { get; set; }
    }
    public class MiniLoaiSpViewMD
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MaLoai { get; set; }
        public string? TenLoai { get; set; }
    }
    public class HangSpViewMD
    {
        [Key]
        public string MaNCC { get; set; }
        public string? TenCongTy { get; set; }
        public string Mota { get; set; }
        public string DiaChi { get; set; }
        public string Email { get; set; }
        public string NguoiLienLac { get; set; }
        public string DienThoai { get; set; }
        public string Logo { get; set; }
    }
    public class MiniHangSpViewMD
    {
        [Key]
        public string MaNCC { get; set; }
        public string? TenCongTy { get; set; }
    }
}
