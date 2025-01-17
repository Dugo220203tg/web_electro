using TrangQuanLy.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace TrangQuanLy.ViewComponents
{
    public class NotificationViewComponent : ViewComponent
    {
        private readonly HttpClient _client;
        Uri baseAddress = new Uri("https://localhost:7109/api");
        public NotificationViewComponent()
        {
            _client = new HttpClient();
            _client.BaseAddress = baseAddress;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            List<NotificationMD> data = new List<NotificationMD>();
            HttpResponseMessage response = await _client.GetAsync(_client.BaseAddress + "/Notification/GetAllNotifications");
            if (response.IsSuccessStatusCode)
            {
                string responseData = await response.Content.ReadAsStringAsync();
                data = JsonConvert.DeserializeObject<List<NotificationMD>>(responseData)
                    .OrderByDescending(n => n.CreateAt)
                    .Take(5)
                    .ToList();
            }
            return View("Index", data);
        }
    }
}
