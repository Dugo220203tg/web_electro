namespace API_Web_Shop_Electronic_TD.Models
{
	public class VnPayMD
	{
	}
	public class PaymentResponseModel
	{
		public bool Success { get; set; }
		public string PaymentMethod { get; set; }
		public string OrderDescription { get; set; }
		public string OrderId { get; set; }
		public string PaymentId { get; set; }
		public string TransactionId { get; set; }
		public string Token { get; set; }
		public string VnPayResponseCode { get; set; }
		public string Message { get; set; }
	}
	public class VnPaymentRequestModel
	{
		public int OrderId { get; set; }
		public string FullName { get; set; }
		public string Description { get; set; }
		public double Amount { get; set; }
		public string OrderType { get; set; }
		public DateTime CreatedDate { get; set; }
	}
	public class PaymentInformationModel
	{
		public int OrderId { get; set; }
		public string OrderType { get; set; }
		public double Amount { get; set; }
		public string Description { get; set; }
		public string FullName { get; set; }
		public DateTime CreatedDate { get; set; }
		public string BankCode { get; set; }

	}
	public class VnPayCallbackQuery
	{
		public string OrderId { get; set; }
		public string TransactionId { get; set; }
		public string PaymentId { get; set; }
		public string OtherField { get; set; } // Thêm các trường khác nếu cần
	}

	public class VnPaymentResponseModel
	{
		public bool Success { get; set; }
		public string PaymentMethod { get; set; }
		public string OrderDescription { get; set; }
		public string OrderId { get; set; }
		public string PaymentId { get; set; }
		public string TransactionId { get; set; }
		public string Token { get; set; }
		public string VnPayResponseCode { get; set; }
		public string Message { get; set; }
	}
}
