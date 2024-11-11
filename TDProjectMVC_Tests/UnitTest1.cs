using NUnit.Framework;
using Moq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using TrangQuanLy.Controllers;
using TrangQuanLy.Models;
using System.Net;
using System.Text;
using Microsoft.AspNetCore.Http;

namespace TDProjectMVC_Tests
{
    public class Tests : IDisposable
    {
        private MockHttpMessageHandler _mockHttpMessageHandler;
        private HttpClient _httpClient;
        private HttpContextAccessor _httpContextAccessor;
        private AdminController _controller;

        [SetUp]
        public void Setup()
        {
            _mockHttpMessageHandler = new MockHttpMessageHandler();
            _httpClient = new HttpClient(_mockHttpMessageHandler) { BaseAddress = new Uri("http://localhost") };
            _httpContextAccessor = new HttpContextAccessor();
            _controller = new AdminController(_httpClient, _httpContextAccessor);
        }

        // Other test methods...

        public void Dispose()
        {
            _httpClient.Dispose();
        }


        [Test]
        public async Task DangNhap_Should_Return_View_When_Response_Is_Unsuccessful()
        {
            var model = new AdminViewModel { UserName = "testuser", Password = "testpassword" };
            _mockHttpMessageHandler.SendAsyncFunc = (request, cancellationToken) =>
            {
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound));
            };

            var result = await _controller.DangNhap(model, null);

            Assert.That(result, Is.InstanceOf<RedirectResult>());
        }

        [Test]
        public async Task DangNhap_Should_Return_View_When_User_Not_Active()
        {
            var model = new AdminViewModel { UserName = "user123", Password = "user123" };
            var responseModel = new AdminViewModel { UserName = "user123", Vaitro = 0 };
            var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonConvert.SerializeObject(responseModel), Encoding.UTF8, "application/json")
            };
            _mockHttpMessageHandler.SendAsyncFunc = (request, cancellationToken) =>
            {
                return Task.FromResult(responseMessage);
            };

            var result = await _controller.DangNhap(model, null);

            Assert.That(result, Is.InstanceOf<RedirectResult>());
        }

        [Test]
        public async Task DangNhap_Should_Redirect_To_ReturnUrl_When_Authentication_Succeeds()
        {
            var model = new AdminViewModel { UserName = "admin22", Password = "admin22" };
            var returnUrl = "/";

            _mockHttpMessageHandler.SendAsyncFunc = (request, cancellationToken) =>
            {
                // Simulate successful authentication
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(JsonConvert.SerializeObject(new { redirectUrl = returnUrl }), Encoding.UTF8, "application/json")
                });
            };

            var result = await _controller.DangNhap(model, returnUrl);

            Assert.That(result, Is.InstanceOf<RedirectResult>());
            var redirectResult = (RedirectResult)result;
            Assert.That(redirectResult.Url, Is.EqualTo("/"));
        }
    }
}