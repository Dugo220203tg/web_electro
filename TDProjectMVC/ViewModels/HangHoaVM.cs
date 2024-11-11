namespace TDProjectMVC.ViewModels
{
    public class HangHoaVM
    {
        public int MaHH { get; set; }
        public string TenHH { get; set; }
        public string Hinh { get; set;}
        public string MoTaNgan { get; set; }
        public string TenLoai { get; set; }
        public int ML { get; set; }
        public double GiamGia { get; set; }
		public string MoTa { get; set; }
		public DateTime NgaySX { get; set; }
        public string MaNCC { get; set; }
        public string NCC { get; set; }
        public double DonGia { get; set; }
        public int SoLuong { get; set; }
        public int DiemDanhGia { get; set; }
        public List<string> ImageUrls { get; set; }

    }
    public class ChiTietHangHoaVM
    {
        public int DiemDanhGia { get; set; }
        public int SoLuong {  get; set; }
        public int MaHH { get; set; }
        public string TenHH { get; set; }
        public string Hinh { get; set; }
        public double DonGia { get; set; }
        public string MoTaNgan { get; set; }
        public string TenLoai { get; set; }
        public double GiamGia { get; set; }
        public string TenAlias { get; set; }
        public string MoTa { get; set; }
        public DateTime NgaySX { get; set; }
        public string MaNCC { get; set; }
        public int SoLanXem { get; set; }
        public string Hang { get; set; }
        public string NCC { get; set; }
        public int ML { get; set; }
        public List<string> GetImageUrls()
        {
            if (string.IsNullOrEmpty(Hinh))
            {
                return new List<string>();
            }
            return Hinh.Split(',').ToList();
        }
    }
    public class HangSpVM
    {
        public string MaNCC { get; set;}
        public string TenCongTy { get; set; }
		public int SoLuong { get; set; }

	}
}
