using API_Web_Shop_Electronic_TD.Models;
using Microsoft.IdentityModel.Tokens;
using System.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace API_Web_Shop_Electronic_TD.Helpers
{
	public class MyUtil
	{
		public static string GenerateRamdomKey(int length = 5)
		{
			var pattern = @"qazwsxedcrfvtgbyhnujmiklopQAZWSXEDCRFVTGBYHNUJMIKLOP!";
			var sb = new StringBuilder();
			var rd = new Random();
			for (int i = 0; i < length; i++)
			{
				sb.Append(pattern[rd.Next(0, pattern.Length)]);
			}

			return sb.ToString();
		}
		public bool VerifyPassword(string inputPassword, string storedPassword, string randomKey)
		{
			string hashedInputPassword = inputPassword.ToMd5Hash(randomKey);
			return hashedInputPassword == storedPassword;
		}
		//private string CreateJWT(KhachHangsMD khachhang)
		//{
		//	// Use a key of at least 32 characters

		//	var claims = new[]
		//	{
		//		new Claim(ClaimTypes.Name, khachhang.HoTen),
		//		new Claim(ClaimTypes.NameIdentifier, khachhang.UserName.ToString())
		//	};
		//	var secretKey = configuration.GetSection("AppSettings:Key").Value;
		//	var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
		//	var signingCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

		//	var tokenDescriptor = new SecurityTokenDescriptor
		//	{
		//		Subject = new ClaimsIdentity(claims),
		//		Expires = DateTime.UtcNow.AddMinutes(30),
		//		SigningCredentials = signingCredentials
		//	};

		//	var tokenHandler = new JwtSecurityTokenHandler();
		//	var token = tokenHandler.CreateToken(tokenDescriptor);

		//	return tokenHandler.WriteToken(token);
		//}
	}
}
