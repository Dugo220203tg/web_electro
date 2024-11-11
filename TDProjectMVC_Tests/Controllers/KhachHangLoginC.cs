using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json;
using NUnit.Framework;
using TDProjectMVC.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using TDProjectMVC.Data;
using TDProjectMVC.Helpers;
using TDProjectMVC.Models;
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace TDProjectMVC_Tests.Controllers
{
    [TestFixture]
    public class KhachHangControllerTests
    {
        private WebApplicationFactory<Program> _factory;

        [OneTimeSetUp]
        public void Setup()
        {
            _factory = new WebApplicationFactory<Program>()
                .WithWebHostBuilder(builder =>
                {
                    builder.ConfigureServices((context, services) =>
                    {
                        // Cấu hình sử dụng cơ sở dữ liệu SQL Server thực tế
                        var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<Hshop2023Context>));
                        if (descriptor != null)
                        {
                            services.Remove(descriptor);
                        }

                        services.AddDbContext<Hshop2023Context>(options =>
                        {
                            options.UseSqlServer(context.Configuration.GetConnectionString("HShop"));
                        });

                        // Tạo và thêm dữ liệu giả lập vào cơ sở dữ liệu
                        var sp = services.BuildServiceProvider();
                        using (var scope = sp.CreateScope())
                        {
                            var scopedServices = scope.ServiceProvider;
                            var db = scopedServices.GetRequiredService<Hshop2023Context>();

                            // Đảm bảo cơ sở dữ liệu hiện tại là sạch sẽ
                            db.Database.EnsureDeleted();
                            db.Database.EnsureCreated();

                            // Thêm người dùng thử nghiệm
                            db.KhachHangs.Add(new KhachHang
                            {
                                MaKh = "admin44",
                                MatKhau = "admin44".ToMd5Hash("randomkey"),
                                VaiTro = 0,
                                HieuLuc = true,
                                Email = "admin44@example.com",
                                HoTen = "Admin User",
                                DiaChi = "123 Test St.",
                                DienThoai = "1234567890",
                                NgaySinh = DateOnly.Parse("2000-01-01"),
                                RandomKey = "randomkey"
                            });
                            db.SaveChanges();
                        }
                    });
                });
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            _factory.Dispose();
        }

        //[Test]
        //public async Task DangNhap_ValidCredentials_ReturnsRedirectToReturnUrl()
        //{
        //    // Arrange
        //    var client = _factory.CreateClient();
        //    var loginViewModel = new LoginVM
        //    {
        //        UserName = "admin44",
        //        Password = "admin44"
        //    };
        //    var content = new StringContent(JsonConvert.SerializeObject(loginViewModel), Encoding.UTF8, "application/json");

        //    // Act
        //    var response = await client.PostAsync("/KhachHang/DangNhap?ReturnUrl=/some-return-url", content);

        //    // Assert
        //    Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Redirect));
        //    Assert.That(response.Headers.Location?.OriginalString, Is.EqualTo("/some-return-url"));
        //}
    }
}
