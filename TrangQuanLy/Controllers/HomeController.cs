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
        public async Task<IActionResult> Index(int? page, int? pageSize)
        {
            try
            {
                // Khởi tạo các giá trị mặc định cho phân trang
                int pageIndex = page ?? 1;
                int pageSizeValue = pageSize ?? 5;
                ViewBag.PageSize = pageSizeValue;

                // Get current month and year
                DateTime currentDate = DateTime.Now;
                int currentMonth = currentDate.Month;
                int currentYear = currentDate.Year;

                // Lấy danh sách hóa đơn
                List<HoaDonViewModel> hoaDonList = new List<HoaDonViewModel>();
                var hoaDonResponse = await _client.GetAsync(_client.BaseAddress + "/HoaDon/GetAll");
                if (hoaDonResponse.IsSuccessStatusCode)
                {
                    string hoaDonData = await hoaDonResponse.Content.ReadAsStringAsync();
                    hoaDonList = JsonConvert.DeserializeObject<List<HoaDonViewModel>>(hoaDonData);
                }

                // Filter for current month statistics
                DateTime firstDayOfMonth = new DateTime(currentYear, currentMonth, 1);
                DateTime lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);
                var currentMonthHoaDonList = hoaDonList.Where(h => h.NgayDat >= firstDayOfMonth && h.NgayDat <= lastDayOfMonth && h.MaTrangThai == 3).ToList();
                var staticHoaDonList = hoaDonList.Where(h => h.MaTrangThai == 0).ToList();
                var TodayHoaDonList = hoaDonList.Where(h => h.NgayDat.Date == DateTime.Today).ToList();

                // Calculate statistics for current month
                var statisticss = new HoaDonStatisticsViewModel
                {
                    NotApprovedOrders = staticHoaDonList.Count,
                    ToDayOrders = TodayHoaDonList.Count,
                    TotalOrders = currentMonthHoaDonList.Count,
                    TotalIncome = (decimal)currentMonthHoaDonList.Sum(h => h.ChiTietHds.Sum(ct => ct.SoLuong * ct.DonGia)),
                    Month = currentMonth,
                    Year = currentYear
                };

                // Lấy dữ liệu thống kê
                var statisticsResponse = await _client.GetAsync(_client.BaseAddress + "/ChiTietHoaDon/Statistics");
                if (statisticsResponse.IsSuccessStatusCode)
                {
                    string statsData = await statisticsResponse.Content.ReadAsStringAsync();
                    var statistics = JsonConvert.DeserializeObject<List<CategorySalesStatistics>>(statsData);

                    // Xử lý dữ liệu cho biểu đồ
                    var months = statistics.Select(s => s.Month).Distinct().OrderBy(m => m).ToList();

                    // Chuẩn bị dữ liệu cho từng danh mục
                    var category1Data = GetCategoryData(statistics, 1, months);
                    var category8Data = GetCategoryData(statistics, 8, months);
                    var category11Data = GetCategoryData(statistics, 11, months);

                    // Truyền dữ liệu thống kê qua ViewBag
                    ViewBag.Labels = months.Select(m => $"Tháng {m}").ToList();
                    ViewBag.SalesData1 = category1Data;
                    ViewBag.SalesData8 = category8Data;
                    ViewBag.SalesData11 = category11Data;
                    ViewBag.Statistics = statisticss;

                }

                // Tạo phân trang
                PaginatedList<HoaDonViewModel> paginatedList = PaginatedList<HoaDonViewModel>.CreateAsync(
                    hoaDonList, pageIndex, pageSizeValue);
                ViewBag.TotalPages = paginatedList.TotalPages;
                ViewBag.Page = pageIndex;

                return View(paginatedList);
            }
            catch (Exception ex)
            {
                // Log lỗi nếu cần
                return View("Error");
            }
        }
        private List<int> GetCategoryData(List<CategorySalesStatistics> statistics, int categoryId, List<int> months)
        {
            return months.Select(month =>
                statistics.FirstOrDefault(s => s.DanhMucId == categoryId && s.Month == month)?.TotalQuantitySold ?? 0
            ).ToList();
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
