using Newtonsoft.Json;

namespace API_Web_Shop_Electronic_TD.Models
{
	public class MomoOptionModel
	{
		public string MomoApiUrl { get; set; }
		public string SecretKey { get; set; }
		public string AccessKey { get; set; }
		public string ReturnUrl { get; set; }
		public string NotifyUrl { get; set; }
		public string PartnerCode { get; set; }
		public string RequestType { get; set; }
	}

	public class MomoCreatePaymentResponseModel
	{
		public string RequestId { get; set; }
		public int ErrorCode { get; set; }
		public string OrderId { get; set; }
		public string Message { get; set; }
		public string LocalMessage { get; set; }
		public string RequestType { get; set; }
		[JsonProperty("payUrl")]
		public string PayUrl { get; set; }
		public string Signature { get; set; }
		public string QrCodeUrl { get; set; }
		public string Deeplink { get; set; }
		public string DeeplinkWebInApp { get; set; }

	}
	public class OrderInfoModel
	{
		public int Id { get; set; }
		public string OrderId { get; set; }
		public string OrderInfo { get; set; }
		public string FullName { get; set; }
		public decimal Amount { get; set; }
		public DateTime DatePaid { get; set; }
	}
	public class MomoExecuteResponseModel
	{
		public string Amount { get; set; }
		public string OrderId { get; set; }
		public string OrderInfo { get; set; }
		public int ResponseCode { get; set; }
		public string Message { get; set; }
		public string PayType { get; set; }
		public long TransId { get; set; }
	}
}
