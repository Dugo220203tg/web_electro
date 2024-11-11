using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using TrangQuanLy.Models;

namespace TrangQuanLy.ViewComponents
{
    [ViewComponent]
    public class TrangThaiHdViewComponent : ViewComponent
    {
        private readonly HttpClient _client;
        Uri baseAddress = new Uri("https://localhost:7109/api");

        public TrangThaiHdViewComponent()
        {
            _client = new HttpClient();
            _client.BaseAddress = baseAddress;
        }

        public IViewComponentResult Invoke(int maTrangThai)
        {
            List<TrangThaiHd> TrangThai = new List<TrangThaiHd>();
            HttpResponseMessage response = _client.GetAsync(_client.BaseAddress + "/TrangThaiHd/GetAll").Result;
            if (response.IsSuccessStatusCode)
            {
                string data = response.Content.ReadAsStringAsync().Result;
                TrangThai = JsonConvert.DeserializeObject<List<TrangThaiHd>>(data).OrderBy(p => p.TenTrangThai).ToList();
            }

            // Pass maTrangThai to the view to determine the selected item
            ViewBag.SelectedTrangThai = maTrangThai;

            return View("Index", TrangThai);
        }

    }
}
