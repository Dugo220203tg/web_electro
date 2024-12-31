namespace TDProjectMVC.ViewModels
{
	public class CartModel
	{
		public string TenHH { get; set; }
		public string Hinh { get; set; }
		public double DonGia { get; set; }
		public int SoLuong { get; set; }
		public int Quantity { get; set; }
		public double Total { get; set; }
	}
    public class CardProduct
    {
        public string TenHH { get; set; }
        public string Hinh { get; set; }
        public double DonGia { get; set; }
        public int SoLuong { get; set; }
        public int Quantity { get; set; }
        public double Total { get; set; }
    }
    public class CartRequest
    {
        public string Makh { get; set; }
        public int MaHh { get; set; }
        public string TenHH { get; set; }
        public string Hinh { get; set; }
        public double DonGia { get; set; }
        public int SoLuong { get; set; }
        public int Quantity { get; set; }
        public double Total { get; set; }
    }
}
