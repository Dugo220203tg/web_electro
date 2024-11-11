using System.ComponentModel.DataAnnotations;

namespace TrangQuanLy.Models
{
    public class DanhGiaSpMD
    {
        [Key]
        public int MaDg { get; set; }
        [Key]
        public string MaKH { get; set; }
        public string NoiDung { get; set; }
        public int Sao { get; set; }
        public DateTime Ngay { get; set; }
        [Key]
        public int MaHH { get; set; }
        public int TrangThai { get; set; }
        public string TenHangHoa { get; set; }
        public string TenKhachHang { get; set; }
        public string HinhAnh { get; set; }
        public string FirstImageUrl
        {
            get
            {
                if (!string.IsNullOrEmpty(HinhAnh))
                {
                    var imageUrls = HinhAnh.Split(',');
                    return imageUrls.FirstOrDefault();
                }
                return string.Empty;
            }
        }
        public string TenTrangThai
        {
            get
            {
                return TrangThai == 1 ? "Hiển Thị" : "Ẩn";
            }
        }
    }
}
