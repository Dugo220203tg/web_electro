using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using TrangQuanLy.Models;

namespace TrangQuanLy.ViewComponents
{
    public class HangSpViewComponent : ViewComponent
    {
        private readonly HttpClient _client;
        Uri baseAddress = new Uri("https://localhost:7109/api");
        public HangSpViewComponent()
        {
            _client = new HttpClient();
            _client.BaseAddress = baseAddress;
        }
        public IViewComponentResult Invoke()
        {
            List<MiniHangSpViewMD> Database = new List<MiniHangSpViewMD>();
            HttpResponseMessage response = _client.GetAsync(_client.BaseAddress + "/HangSp/GetAll").Result;
            if (response.IsSuccessStatusCode)
            {
                string data = response.Content.ReadAsStringAsync().Result;
                Database = JsonConvert.DeserializeObject<List<MiniHangSpViewMD>>(data).OrderBy(p => p.TenCongTy).ToList();
            }
            return View("Index", Database);
        }
    }
}
