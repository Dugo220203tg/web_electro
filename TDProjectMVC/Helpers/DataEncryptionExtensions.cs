﻿using System.Security.Cryptography;
using System.Text;

namespace TDProjectMVC.Helpers
{
	public static class DataEncryptionExtensions
	{
		#region [Hashing Extension]
		public static string ToSHA256Hash(this string password, string? saltKey)
		{
			var sha256 = SHA256.Create();
			byte[] encryptedSHA256 = sha256.ComputeHash(Encoding.UTF8.GetBytes(string.Concat(password, saltKey)));
			sha256.Clear();

			return Convert.ToBase64String(encryptedSHA256);
		}

		public static string ToSHA512Hash(this string password, string? saltKey)
		{
			SHA512Managed sha512 = new SHA512Managed();
			byte[] encryptedSHA512 = sha512.ComputeHash(Encoding.UTF8.GetBytes(string.Concat(password, saltKey)));
			sha512.Clear();

			return Convert.ToBase64String(encryptedSHA512);
		}

        public static string ToMd5Hash(this string password, string? saltKey)
        {
            // Kiểm tra cả password và saltKey
            if (string.IsNullOrEmpty(password))
            {
                throw new ArgumentNullException(nameof(password), "Password cannot be null or empty.");
            }

            if (string.IsNullOrEmpty(saltKey))
            {
                throw new ArgumentNullException(nameof(saltKey), "Salt key cannot be null or empty.");
            }

            using (var md5 = MD5.Create())
            {
                byte[] data = md5.ComputeHash(Encoding.UTF8.GetBytes(string.Concat(password, saltKey)));
                StringBuilder sBuilder = new StringBuilder();
                for (int i = 0; i < data.Length; i++)
                {
                    sBuilder.Append(data[i].ToString("x2"));
                }
                return sBuilder.ToString();
            }
        }

        #endregion

    }

}