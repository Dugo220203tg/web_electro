using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using TrangQuanLy.Models;

namespace TrangQuanLy.ViewComponents
{
    public class LoaiSpViewComponent : ViewComponent
    {
        private readonly HttpClient _client;
        Uri baseAddress = new Uri("https://localhost:7109/api");
        public LoaiSpViewComponent()
        {
            _client = new HttpClient();
            _client.BaseAddress = baseAddress;
        }
        public IViewComponentResult Invoke()
        {
            List<MiniLoaiSpViewMD> LoaiSP = new List<MiniLoaiSpViewMD>();
            HttpResponseMessage response = _client.GetAsync(_client.BaseAddress + "/LoaiSp/GetAll").Result;
            if (response.IsSuccessStatusCode)
            {
                string data = response.Content.ReadAsStringAsync().Result;
                LoaiSP = JsonConvert.DeserializeObject<List<MiniLoaiSpViewMD>>(data).OrderBy(p => p.TenLoai).ToList();
            }
            return View("Index", LoaiSP);
        }
    }
}
