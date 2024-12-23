namespace API_Web_Shop_Electronic_TD.DTOs
{
	public class AuthResponseDto
	{
		public string Token { get; set; }
		public bool IsSuccess { get; set; }
		public string Message { get; set; }
		public string RefreshToken { get; set; }
	}
}
