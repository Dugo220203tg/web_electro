using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;
using TrangQuanLy.Models;
using Newtonsoft.Json;
using System.Text;
using System.Net.Http;
using Microsoft.AspNetCore.Authorization;
using PagedList;
using TrangQuanLy.Helpers;

namespace TrangQuanLy.Controllers
{
	public class HomeController : Controller
	{
        private readonly HttpClient _client;
        Uri baseAddress = new Uri("https://localhost:7109/api");
        public HomeController(ILogger<HomeController> logger)
		{
            _client = new HttpClient();
            _client.BaseAddress = baseAddress;
        }
		public IActionResult Privacy()
		{
			return View();
        }
        [Authorize]
        [HttpGet]
        public IActionResult Index(int? page, int? pageSize)
        {
            int pageIndex = page ?? 1;
            int pageSizeValue = pageSize ?? 5;
            ViewBag.PageSize = pageSizeValue;

            // Get current month and year
            DateTime currentDate = DateTime.Now;
            int currentMonth = currentDate.Month;
            int currentYear = currentDate.Year;

            List<HoaDonViewModel> allHoaDonList = new List<HoaDonViewModel>();
            HttpResponseMessage response = _client.GetAsync(_client.BaseAddress + "/HoaDon/GetAll").Result;
            if (response.IsSuccessStatusCode)
            {
                string data = response.Content.ReadAsStringAsync().Result;
                allHoaDonList = JsonConvert.DeserializeObject<List<HoaDonViewModel>>(data);
            }

            // Filter for current month statistics
            DateTime firstDayOfMonth = new DateTime(currentYear, currentMonth, 1);
            DateTime lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);
            var currentMonthHoaDonList = allHoaDonList.Where(h => h.NgayDat >= firstDayOfMonth && h.NgayDat <= lastDayOfMonth && h.MaTrangThai == 3).ToList();
            var staticHoaDonList = allHoaDonList.Where(h => h.NgayDat >= firstDayOfMonth && h.NgayDat <= lastDayOfMonth && h.MaTrangThai == 0).ToList();
            var TodayHoaDonList = allHoaDonList.Where(h => h.NgayDat.Date == DateTime.Today).ToList();

            // Calculate statistics for current month
            var statistics = new HoaDonStatisticsViewModel
            {
                NotApprovedOrders = staticHoaDonList.Count,
                ToDayOrders = TodayHoaDonList.Count,
                TotalOrders = currentMonthHoaDonList.Count,
                TotalIncome = (decimal)currentMonthHoaDonList.Sum(h => h.ChiTietHds.Sum(ct => ct.SoLuong * ct.DonGia)),
                Month = currentMonth,
                Year = currentYear
            };

            // Calculate product count sold in the last 6 months for MaDanhMuc = 1, 8, 11
            DateTime sixMonthsAgo = currentDate.AddMonths(-6);

            // Tạo danh sách các tháng trong 6 tháng gần nhất
            var monthRange = Enumerable.Range(0, 6)
                .Select(i => currentDate.AddMonths(-i))
                .Select(d => new { Year = d.Year, Month = d.Month })
                .OrderBy(d => d.Year)
                .ThenBy(d => d.Month)
                .ToList();

            // Lấy dữ liệu bán hàng và đảm bảo có đủ dữ liệu cho mọi tháng
            var categoryStatistics = allHoaDonList
                .Where(h => h.NgayDat >= sixMonthsAgo && h.NgayDat <= currentDate)
                .SelectMany(h => h.ChiTietHds, (hoaDon, chiTiet) => new { hoaDon.NgayDat, chiTiet.MaDanhMuc, chiTiet.SoLuong })
                .Where(ct => ct.MaDanhMuc == 1 || ct.MaDanhMuc == 8 || ct.MaDanhMuc == 11)
                .GroupBy(ct => new { ct.MaDanhMuc, Year = ct.NgayDat.Year, Month = ct.NgayDat.Month })
                .Select(g => new
                {
                    MaDanhMuc = g.Key.MaDanhMuc,
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    TotalSold = g.Sum(ct => ct.SoLuong)
                })
                .ToList();

            // Tạo dữ liệu đầy đủ cho mỗi tháng và mỗi danh mục
            var completeData = from month in monthRange
                               from category in new[] { 1, 8, 11 }
                               join stats in categoryStatistics
                               on new { month.Year, month.Month, MaDanhMuc = category }
                               equals new { stats.Year, stats.Month, stats.MaDanhMuc }
                               into gj
                               from subStats in gj.DefaultIfEmpty()
                               select new
                               {
                                   month.Year,
                                   month.Month,
                                   MaDanhMuc = category,
                                   TotalSold = subStats?.TotalSold ?? 0
                               };

            // Chuẩn bị dữ liệu cho biểu đồ
            var labels = monthRange.Select(x => $"{x.Month}/{x.Year}").ToArray();

            var salesData1 = completeData.Where(x => x.MaDanhMuc == 1)
                .OrderBy(x => x.Year).ThenBy(x => x.Month)
                .Select(x => x.TotalSold).ToArray();

            var salesData8 = completeData.Where(x => x.MaDanhMuc == 8)
                .OrderBy(x => x.Year).ThenBy(x => x.Month)
                .Select(x => x.TotalSold).ToArray();

            var salesData11 = completeData.Where(x => x.MaDanhMuc == 11)
                .OrderBy(x => x.Year).ThenBy(x => x.Month)
                .Select(x => x.TotalSold).ToArray();

            ViewBag.Labels = labels;
            ViewBag.SalesData1 = salesData1;
            ViewBag.SalesData8 = salesData8;
            ViewBag.SalesData11 = salesData11;

            PaginatedList<HoaDonViewModel> paginatedList = PaginatedList<HoaDonViewModel>.CreateAsync(
                allHoaDonList, pageIndex, pageSizeValue);
            ViewBag.TotalPages = paginatedList.TotalPages;
            ViewBag.Page = pageIndex;
            ViewBag.Statistics = statistics;

            return View(paginatedList);
        }

        [Authorize]
        [HttpGet]
        public IActionResult Edit(int id)
        {
            try
            {
                HoaDonViewModel hoadon = new HoaDonViewModel();
                HttpResponseMessage response = _client.GetAsync(_client.BaseAddress + "/HoaDon/GetById/" + id).Result;
                if (response.IsSuccessStatusCode)
                {
                    string data = response.Content.ReadAsStringAsync().Result;
                    hoadon = JsonConvert.DeserializeObject<HoaDonViewModel>(data);
                }
                return View(hoadon);
            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
                return View();
            }
        }
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Edit(HoaDonViewModel model, int MaHD)
        {
            try
            {
                string data = JsonConvert.SerializeObject(model);
                StringContent content = new StringContent(data, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await _client.PutAsync(_client.BaseAddress + "/HoaDon/Update/" + MaHD, content);
                if (response.IsSuccessStatusCode)
                {
                    TempData["success"] = "Hóa đơn đã được cập nhật!";
                    return RedirectToAction("Index");
                }
                return View(model);
            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
                return View(model);
            }
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Search(string? query)
        {
            // Initialize HangHoaVM list to store search results
            List<HoaDonViewModel> searchResult = new List<HoaDonViewModel>();

            // Send a request to the API to get all HangHoa entities
            List<HoaDonViewModel> HoaDon = new List<HoaDonViewModel>();
            HttpResponseMessage response = _client.GetAsync(_client.BaseAddress + "/HoaDon/GetAll").Result;

            if (response.IsSuccessStatusCode)
            {
                string data = response.Content.ReadAsStringAsync().Result;
                HoaDon = JsonConvert.DeserializeObject<List<HoaDonViewModel>>(data);
            }
            else
            {
                return View("Error");
            }
            if (query != null)
            {
                searchResult = HoaDon.Where(h => h.HoTen.Contains(query) ).ToList();
                return View(searchResult);
            }
            if (query == null)
            {
                return View(HoaDon);
            }
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }


    }
}
