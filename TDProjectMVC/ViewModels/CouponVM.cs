namespace TDProjectMVC.ViewModels
{
    public class CouponVM
    {
        public int id {  get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Quantity { get; set; }
        public DateTime DateEnd { get; set; }
        public DateTime DateStart { get; set; }
        public decimal price { get; set; }  
        public int Status { get; set; }
    }
}
