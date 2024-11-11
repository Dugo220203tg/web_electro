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
	}
}
