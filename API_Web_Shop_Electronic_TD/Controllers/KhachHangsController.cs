using Microsoft.AspNetCore.Mvc;
using API_Web_Shop_Electronic_TD.Data;
using API_Web_Shop_Electronic_TD.Interfaces;
using API_Web_Shop_Electronic_TD.Mappers;
using API_Web_Shop_Electronic_TD.Models;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authorization;
using System.Security.Cryptography;
using System.Net;
using RestSharp;
using AuthResponseDto = API_Web_Shop_Electronic_TD.Models.AuthResponseDto;
using ErrorResponse = API_Web_Shop_Electronic_TD.Models.ErrorResponse;

namespace API_Web_Shop_Electronic_TD.Controllers
{
	[Route("api/[controller]/[action]")]
	[ApiController]
	public class KhachHangsController : Controller
	{
		private readonly IKhachHangRepository KhachHangsRepository;
		private readonly Hshop2023Context db;
		private readonly IConfiguration configuration;


		public KhachHangsController(IKhachHangRepository KhachHangsRepository, IConfiguration configuration, Hshop2023Context db)
		{
			this.KhachHangsRepository = KhachHangsRepository;
			this.db = db;
			this.configuration = configuration;
		}
		#region ------ CONTROLLER FOR ADMIN ELECTRO WEB ------
		[HttpPost("UpdatePassword")]
		public async Task<IActionResult> UpdatePassword([FromBody] UpdatePasswordDTO model)
		{
			if (string.IsNullOrWhiteSpace(model.CodeSend) || string.IsNullOrWhiteSpace(model.NewPassword))
			{
				return BadRequest(new { message = "Mật khẩu hiện tại và mật khẩu mới không được để trống" });
			}

			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			var khachHang = await KhachHangsRepository.UpdatePasswordAsync(model.CodeSend, model.NewPassword, model.Email);
			if (khachHang == null)
			{
				return Unauthorized(new { message = "Email hoặc mã xác nhận không đúng" });
			}

			return Ok(new { message = "Cập nhật mật khẩu thành công" });
		}

		[HttpGet]
		public async Task<IActionResult> GetAll()
		{
			try
			{
				var khachHangsMD = await KhachHangsRepository.GetAllAsync();
				var model = khachHangsMD.Select(k => k.ToKhachHangDo()).ToList();

				return Ok(model);
			}
			catch (Exception ex)
			{
				// Xử lý và thông báo lỗi
				return BadRequest("Đã xảy ra lỗi: " + ex.ToString());
			}
		}

		[HttpGet("{MaKh}")]
		public async Task<IActionResult> GetById([FromRoute] string MaKh)
		{
			if (!ModelState.IsValid)
				return BadRequest(ModelState);

			var khachhang = await KhachHangsRepository.GetByIdAsync(MaKh);

			if (khachhang == null)
			{
				return NotFound(new ErrorResponse
				{
					Message = "Không tìm thấy dữ liệu",
					Errors = new List<string> { "Không tìm thấy thông tin với mã đã cho" }
				});
			}

			return Ok(khachhang.ToKhachHangDo());
		}

		[HttpPost]
		public async Task<IActionResult> Post(AdminDkMD model)
		{
			try
			{
				if (!ModelState.IsValid)
				{
					var errors = ModelState.Values.SelectMany(v => v.Errors)
												   .Select(e => e.ErrorMessage)
												   .ToList();
					return BadRequest(new ErrorResponse
					{
						Message = "Dữ liệu không hợp lệ",
						Errors = errors
					});
				}

				var createdModel = await KhachHangsRepository.CreateAsync(model);
				return Ok(createdModel);
			}
			catch (ArgumentException ex)
			{
				return BadRequest(new ErrorResponse
				{
					Message = "Dữ liệu không hợp lệ",
					Errors = new List<string> { ex.Message }
				});
			}
			catch (Exception ex)
			{
				return BadRequest(new ErrorResponse
				{
					Message = "Đã xảy ra lỗi",
					Errors = new List<string> { ex.Message }
				});
			}
		}

		[HttpPut]
		[Route("{MaKh}")]
		public async Task<IActionResult> Update([FromRoute] string MaKh, [FromBody] UpdateKH model)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			try
			{
				var updatedKhachHang = await KhachHangsRepository.UpdateAdminAsync(MaKh, model);
				var result = new
				{
					Email = updatedKhachHang.Email,
					NgaySinh = updatedKhachHang.NgaySinh,
					DiaChi = updatedKhachHang.DiaChi,
					DienThoai = updatedKhachHang.DienThoai
				};

				return Ok(new { message = "Cập nhật thông tin thành công", khachHang = result });
			}
			catch (KeyNotFoundException ex)
			{
				return NotFound(new { message = ex.Message });
			}
			catch (Exception ex)
			{
				// Log the exception here
				return StatusCode(500, new { message = "Đã xảy ra lỗi trong quá trình xử lý", error = ex.Message });
			}
		}

		[HttpPost]
		public IActionResult UpdateTrangThai([FromBody] UpdateHieuLucVMD model)
		{
			try
			{
				var khachhangs = db.KhachHangs.Find(model.UserName);
				if (khachhangs != null)
				{
					if (model.HieuLuc == false)
					{
						khachhangs.HieuLuc = false;
						db.SaveChanges();
						return Ok(new { message = "Đã khóa sử dụng tài khoản !" });
					}
					else if (model.HieuLuc == true)
					{
						khachhangs.HieuLuc = true;
						db.SaveChanges();
						return Ok(new { message = "Đã mở khóa tài khoản !" });
					}
				}
				return NotFound(new { message = "Lỗi chưa xác định." });
			}
			catch (Exception ex)
			{
				return StatusCode(500, ex.Message);
			}
		}

		[HttpDelete("Delete/{maKh}")]
		public async Task<IActionResult> Delete([FromRoute] string maKh)
		{
			Console.WriteLine($"API Delete method called with maKh: {maKh}");
			try
			{
				if (string.IsNullOrEmpty(maKh))
				{
					Console.WriteLine("maKh is null or empty");
					return BadRequest(new { success = false, message = "Mã khách hàng không hợp lệ." });
				}

				var deleteKH = await KhachHangsRepository.DeleteAsync(maKh);

				if (deleteKH == null)
				{
					Console.WriteLine($"Customer not found with maKh: {maKh}");
					return NotFound(new { success = false, message = "Không tìm thấy khách hàng." });
				}

				Console.WriteLine($"Successfully deleted customer with maKh: {maKh}");
				return Ok(new { success = true, message = "Xóa thành công." });
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error in Delete method: {ex}");
				return StatusCode(500, new { success = false, message = $"Lỗi server: {ex.Message}" });
			}
		}

		#endregion --- END ---
		#region ----- CONTROLLER FOR ANGULAR WEB ------ 
		#region ----- LOGIN AND REFRESH TOKEN ANGULAR ------
		[HttpPost]
		public async Task<IActionResult> Login([FromBody] LoginDTO model)
		{
			if (string.IsNullOrWhiteSpace(model.Email))
			{
				return BadRequest(new { message = "UserName không được để trống" });
			}

			if (string.IsNullOrWhiteSpace(model.Password))
			{
				return BadRequest(new { message = "Mật khẩu không được để trống" });
			}

			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			var khachHang = await KhachHangsRepository.GetByEmailAsync(model.Email);

			if (khachHang == null)
			{
				return Unauthorized(new { message = "Email hoặc mật khẩu không đúng" });
			}
			var user = await KhachHangsRepository.Authenticate(model.Email, model.Password);
			if (user == null)
			{
				return Unauthorized("Invalid username or password");
			}
			var token = GenerateToken(khachHang);

			var refreshToken = GenerateRefreshToken();
			_ = int.TryParse(configuration.GetSection("JWTSetting").GetSection("RefreshTokenValidityIn").Value!, out int RefreshTokenValidityIn);
			user.RefreshToken = refreshToken;
			user.RefreshTokenExpiryTime = DateTime.UtcNow.AddMinutes(RefreshTokenValidityIn);
			await KhachHangsRepository.UpdateAsync(user);

			var loginRs = new LoginResDto
			{
				Message = "Login success!",
				Token = token,
				isSuccess = true,
				RefreshToken = refreshToken,
			};
			return Ok(loginRs);
		}

		[HttpPost]
		public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenDTO tokenDto)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}
			var principal = GetPrincipalFromExpiredToken(tokenDto.Token);
			var user = await KhachHangsRepository.GetByEmailAsync(tokenDto.Email);
			if (principal is null || user is null || user.RefreshToken != tokenDto.RefreshToken || user.RefreshTokenExpiryTime <= DateTime.UtcNow)

				return BadRequest(new AuthResponseDto
				{
					IsSuccess = false,
					Message = "Invalid client request"
				});
			var newJwtToken = GenerateToken(user);
			var newRefreshToken = GenerateRefreshToken();
			_ = int.TryParse(configuration.GetSection("JWTSetting").GetSection("RefreshTokenValidityIn").Value!, out int RefreshTokenValidityIn);
			user.RefreshToken = newRefreshToken;
			user.RefreshTokenExpiryTime = DateTime.UtcNow.AddMinutes(RefreshTokenValidityIn);
			await KhachHangsRepository.UpdateAsync(user);
			return Ok(new AuthResponseDto
			{
				IsSuccess = true,
				Token = newJwtToken,
				RefreshToken = newRefreshToken,
				Message = " Refresh token successfully"
			});
		}

		private object GetPrincipalFromExpiredToken(string token)
		{
			var tokenParameters = new TokenValidationParameters
			{
				ValidateAudience = false,
				ValidateIssuer = false,
				ValidateIssuerSigningKey = true,
				IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration.GetSection("JWTSetting").GetSection("securityKey").Value!)),
				ValidateLifetime = false,
			};

			var tokenHandler = new JwtSecurityTokenHandler();
			var principal = tokenHandler.ValidateToken(token, tokenParameters, out SecurityToken securityToken);
			if (securityToken is not JwtSecurityToken jwtSecurityToken || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
				throw new SecurityTokenException("Invalid token");
			return principal;
		}

		private string GenerateRefreshToken()
		{
			var randomNumber = new byte[32];
			using var rng = RandomNumberGenerator.Create();
			rng.GetBytes(randomNumber);
			return Convert.ToBase64String(randomNumber);
		}


		private string GenerateToken(KhachHang khachHang)
		{
			var tokenHandler = new JwtSecurityTokenHandler();

			// Retrieve the security key from configuration
			var key = Encoding.ASCII.GetBytes(configuration.GetSection("JWTSetting").GetSection("securityKey").Value
											   ?? throw new ArgumentNullException("Security key is missing in configuration."));

			// Retrieve roles using the repository
			var roles = KhachHangsRepository.GetRoleAsync(khachHang).Result; // Ensure `GetRoleAsync` is correctly implemented

			// Prepare claims
			var claims = new List<Claim>
			{
				new(JwtRegisteredClaimNames.Email, khachHang.Email ?? string.Empty),
				new(JwtRegisteredClaimNames.Name, khachHang.HoTen ?? string.Empty),
				new(JwtRegisteredClaimNames.NameId, khachHang.MaKh ?? string.Empty),
				new(JwtRegisteredClaimNames.Aud, configuration.GetSection("JWTSetting").GetSection("validAudience").Value
											   ?? throw new ArgumentNullException("Valid audience is missing in configuration.")),
				new(JwtRegisteredClaimNames.Iss, configuration.GetSection("JWTSetting").GetSection("validIssuer").Value
											   ?? throw new ArgumentNullException("Valid issuer is missing in configuration."))
			};

			// Add roles to claims
			if (roles != null)
			{
				foreach (var role in roles)
				{
					claims.Add(new Claim(ClaimTypes.Role, role));
				}
			}

			// Define token descriptor
			var tokenDescriptor = new SecurityTokenDescriptor
			{
				Subject = new ClaimsIdentity(claims),
				Expires = DateTime.UtcNow.AddHours(1),
				SigningCredentials = new SigningCredentials(
					new SymmetricSecurityKey(key),
					SecurityAlgorithms.HmacSha256)
			};

			// Generate token
			var token = tokenHandler.CreateToken(tokenDescriptor);
			return tokenHandler.WriteToken(token);
		}
		#endregion ----- end ------
		#region----- FORGOT PASSWORD ------
		[AllowAnonymous]
		[HttpPost("forgot-password")]
		public async Task<ActionResult> ForgotPassword(ForgotPasswordDto forgotPasswordDto)
		{
			var user = await KhachHangsRepository.GetByEmailAsync(forgotPasswordDto.Email);
			if (user is null)
			{
				return Ok(new ResponeseDtoForgotPassword
				{
					IsSuccess = false,
					Message = "User does not exist with this email"
				});
			}

			var token = await KhachHangsRepository.GeneratePasswordResetTokenAsync(user);
			var resetLink = $"http://localhost:4200/reset-password?email={user.Email}&token={WebUtility.UrlEncode(token)}";

			try
			{
				var client = new RestClient("https://send.api.mailtrap.io/api/send");
				var request = new RestRequest();
				request.Method = RestSharp.Method.Post;
				request.AddHeader("Authorization", "Bearer eedd2f654d28e528b7508c9a835d9a9d");
				request.AddHeader("Content-Type", "application/json");
				request.AddJsonBody(new
				{
					from = new { email = "hello@demomailtrap.com", name = "Password Reset" },
					to = new[] { new { email = user.Email } },
					template_uuid = "79281827-2396-4b03-a3df-dd989dcf2db0",
					template_variables = new
					{
						user_email = user.Email,
						pass_reset_link = resetLink
					}
				});

				var response = client.Execute(request);

				if (response.IsSuccessful)
				{
					return Ok(new ResponeseDtoForgotPassword
					{
						IsSuccess = true,
						Message = "Email sent with password reset link. Please check your email."
					});
				}
				if (!response.IsSuccessful)
				{
					Console.WriteLine($"Error Status Code: {response.StatusCode}");
					Console.WriteLine($"Error Message: {response.ErrorMessage}");
					Console.WriteLine($"Error Content: {response.Content}");
					return BadRequest(new AuthResponseDto
					{
						IsSuccess = false,
						Message = $"Failed to send email. {response.Content}"
					});
				}
				else
				{
					return BadRequest(new ResponeseDtoForgotPassword
					{
						IsSuccess = false,
						Message = $"Failed to send email. {response.ErrorMessage}"
					});
				}
			}
			catch (Exception ex)
			{
				return StatusCode(500, new ResponeseDtoForgotPassword
				{
					IsSuccess = false,
					Message = $"Error sending email: {ex.Message}"
				});
			}
		}
		#endregion-----end-----
		#region----- RESET PASSWORD ACCOUNT ANGULAR -----
		[HttpPost]
		[AllowAnonymous]
		public async Task<IActionResult> ResetPassword(ResetPasswordDto resetPasswordDto)
		{
			var user = await KhachHangsRepository.GetByEmailAsync(resetPasswordDto.Email);
			resetPasswordDto.Token = WebUtility.UrlDecode(resetPasswordDto.Token);

			if (user == null)
			{
				return BadRequest(new ResponeseDtoForgotPassword
				{
					IsSuccess = false,
					Message = "User does not exist with this email."
				});
			}

			try
			{
				var result = await KhachHangsRepository.ResetPasswordAsync(user, resetPasswordDto.Token, resetPasswordDto.NewPassword);

				if (result)
				{
					return Ok(new ResponeseDtoForgotPassword
					{
						IsSuccess = true,
						Message = "Password reset successfully."
					});
				}
			}
			catch (Exception ex)
			{
				return BadRequest(new ResponeseDtoForgotPassword
				{
					IsSuccess = false,
					Message = ex.Message
				});
			}

			return BadRequest(new ResponeseDtoForgotPassword
			{
				IsSuccess = false,
				Message = "Password reset failed."
			});
		}
		#endregion-----end -----
		#region ----- GET ACCOUNT DETAIL -----
		[Authorize]
		[HttpGet]
		public async Task<IActionResult> GetAccountDetail()
		{
			var userDetailId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			var user = await KhachHangsRepository.GetByIdAsync(userDetailId!);
			if (user is null)
			{
				return NotFound(new AuthResponseDto
				{
					IsSuccess = false,
					Message = "User not found"
				});

			}
			return Ok(new DetailAccoutResponse
			{
				fullname = user.MaKh,
				email = user.Email,
				roles = user.VaiTro,
				phoneNumber = user.DienThoai,
				accessFailedCount = user.VaiTro
			});
		}
		#endregion----- end -----
		
		#region ----- REGISTER ACCOUNT ANGULAR -----
		[HttpPost]
		public async Task<IActionResult> RegisterAccount([FromBody] RegisterMD model)
		{
			try
			{
				if (!ModelState.IsValid)
				{
					var errors = ModelState.Values
						.SelectMany(v => v.Errors)
						.Select(e => e.ErrorMessage)
						.ToList();

					return BadRequest(new ErrorResponse
					{
						IsSuccess = false,
						Message = "Dữ liệu đăng ký không hợp lệ",
						Errors = errors
					});
				}

				// Kiểm tra email tồn tại
				var existingUser = await KhachHangsRepository.GetByEmailAsync(model.Email);
				if (existingUser != null)
				{
					return BadRequest(new ErrorResponse
					{
						IsSuccess = false,
						Message = "Đăng ký không thành công",
						Errors = new List<string> { "Email đã được sử dụng" }
					});
				}

				// Tạo tài khoản mới
				var newUser = await KhachHangsRepository.CreateAccountAsync(model);

				// Tạo token cho user mới
				var token = CreateJwtToken(newUser);

				return Ok(new AuthResponseDto
				{
					IsSuccess = true,
					Message = "Đăng ký tài khoản thành công",
					Token = token,
					Email = newUser.Email
				});
			}
			catch (ArgumentException ex)
			{
				return BadRequest(new ErrorResponse
				{
					IsSuccess = false,
					Message = "Đăng ký không thành công",
					Errors = new List<string> { ex.Message }
				});
			}
			catch (Exception ex)
			{
				return StatusCode(500, new ErrorResponse
				{
					IsSuccess = false,
					Message = "Đã xảy ra lỗi trong quá trình đăng ký",
					Errors = new List<string> { ex.Message }
				});
			}
		}

		private string CreateJwtToken(KhachHang khachHang)
		{
			var tokenHandler = new JwtSecurityTokenHandler();
			var key = Encoding.ASCII.GetBytes(configuration["JWTSetting:securityKey"]);

			var roles = KhachHangsRepository.GetRoleAsync(khachHang).Result;

			var claims = new List<Claim>
		{
			new(JwtRegisteredClaimNames.Email, khachHang.Email ?? string.Empty),
			new(JwtRegisteredClaimNames.Name, khachHang.HoTen ?? string.Empty),
			new(JwtRegisteredClaimNames.NameId, khachHang.MaKh ?? string.Empty),
			new(JwtRegisteredClaimNames.Aud, configuration["JWTSetting:validAudience"]),
			new(JwtRegisteredClaimNames.Iss, configuration["JWTSetting:validIssuer"])
		};

			if (roles != null)
			{
				foreach (var role in roles)
				{
					claims.Add(new Claim(ClaimTypes.Role, role));
				}
			}

			var tokenDescriptor = new SecurityTokenDescriptor
			{
				Subject = new ClaimsIdentity(claims),
				Expires = DateTime.UtcNow.AddHours(1),
				SigningCredentials = new SigningCredentials(
					new SymmetricSecurityKey(key),
					SecurityAlgorithms.HmacSha256)
			};

			var token = tokenHandler.CreateToken(tokenDescriptor);
			return tokenHandler.WriteToken(token);
		}
		#endregion ------- END -------
		#region ----- CHANGE PASSWORD ACCOUNT ANGULAR -----
		[HttpPost]
		public async Task<IActionResult> ChangePassword(ChangePasswordDto changePasswordDto)
		{
			var user = await KhachHangsRepository.GetByEmailAsync(changePasswordDto.Email);
			if (user == null)
			{
				return BadRequest(new ResponeseDtoForgotPassword
				{
					IsSuccess = false,
					Message = "User does not exist with this email!"
				});
			}

			var result = await KhachHangsRepository.ChangePasswordAsync(user, changePasswordDto.CurrentPassword, changePasswordDto.NewPassword);

			if (result.IsSuccess)
			{
				return Ok(new ResponeseDtoForgotPassword
				{
					IsSuccess = true,
					Message = "Password changed successfully"
				});
			}
			else
			{
				return BadRequest(new ResponeseDtoForgotPassword
				{
					IsSuccess = false,
					Message = result.Message
				});
			}
		}
		#endregion ------End -------
		#endregion
		//private string CreateJWT(KhachHang khachHang)
		//{
		//	var claims = new[]
		//	{
		//		new Claim(ClaimTypes.Name, khachHang.HoTen),
		//		new Claim(ClaimTypes.NameIdentifier, khachHang.MaKh.ToString())
		//	};
		//	var secretKey = configuration.GetSection("JWTSetting:securityKey").Value;
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
