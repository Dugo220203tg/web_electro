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
//using PagedList;
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

                DateTime sixMonthsAgo = currentDate.AddMonths(-5); // Lấy 6 tháng (tháng hiện tại và 5 tháng trước)

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
                var statisticsResponse = await _client.GetAsync(_client.BaseAddress + "/ThongKe/Statistics");
                if (statisticsResponse.IsSuccessStatusCode)
                {
                    string statsData = await statisticsResponse.Content.ReadAsStringAsync();
                    var statistics = JsonConvert.DeserializeObject<List<CategorySalesStatistics>>(statsData);

                    var statisticsData = statistics.Where(s =>
                        new DateTime(currentYear, s.Month, 1) >= new DateTime(sixMonthsAgo.Year, sixMonthsAgo.Month, 1)
                        &&
                        new DateTime(currentYear, s.Month, 1) <= new DateTime(currentYear, currentMonth, 1)
                    ).ToList();
                    // Xử lý dữ liệu cho biểu đồ
                    var months = Enumerable.Range(0, 6)
                        .Select(i => currentDate.AddMonths(-i))
                        .OrderBy(d => d)
                        .Select(d => d.Month)
                        .ToList();
                    // Chuẩn bị dữ liệu cho từng danh mục
                    var category1Data = GetCategoryData(statisticsData, 1, months);
                    var category8Data = GetCategoryData(statistics, 8, months);
                    var category11Data = GetCategoryData(statistics, 11, months);
                    var labels = months.Select(m => $"Tháng {m}").ToList();

                    ViewBag.Labels = labels;


                    // Truyền dữ liệu thống kê qua ViewBag
                    ViewBag.Labels = months.Select(m => $"Tháng {m}").ToList();
                    ViewBag.SalesData1 = category1Data;
                    ViewBag.SalesData8 = category8Data;
                    ViewBag.SalesData11 = category11Data;
                    ViewBag.Statistics = statisticss;

                }
                var DataSellProduct = await _client.GetAsync(_client.BaseAddress + "/ThongKe/DataSellProduct");

                if (DataSellProduct.IsSuccessStatusCode)
                {
                    // Read and deserialize the response
                    string datasell = await DataSellProduct.Content.ReadAsStringAsync();
                    var statics = JsonConvert.DeserializeObject<List<DataSellProduct>>(datasell);

                    // Pass data to ViewBag
                    ViewBag.DataSellProduct = statics;
                }
                else
                {
                    // Handle error response or set default data
                    ViewBag.DataSellProduct = new List<DataSellProduct>();
                    ViewBag.ErrorMessage = "Unable to retrieve sales data.";
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
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> ShowAll(int? page, int? pagesize, int? maTrangThai, string sortOrder)
        {
            if (page == null) page = 1;
            if (pagesize == null) pagesize = 7;

            ViewBag.PageSize = pagesize;
            ViewBag.CurrentSort = sortOrder;
            ViewBag.DateSortParm = string.IsNullOrEmpty(sortOrder) ? "date_desc" : "";
            ViewBag.MaTrangThai = maTrangThai;

            // Get order statuses for dropdown
            List<TrangThaiHoaDonVM> trangThaiList = new List<TrangThaiHoaDonVM>();
            HttpResponseMessage statusResponse = await _client.GetAsync(_client.BaseAddress + "/TrangThaiHd/GetAll");
            if (statusResponse.IsSuccessStatusCode)
            {
                string statusData = await statusResponse.Content.ReadAsStringAsync();
                trangThaiList = JsonConvert.DeserializeObject<List<TrangThaiHoaDonVM>>(statusData);
            }
            ViewBag.TrangThaiHoaDon = trangThaiList;

            // Get all orders
            List<HoaDonViewModel> hoadon = new List<HoaDonViewModel>();
            HttpResponseMessage response = await _client.GetAsync(_client.BaseAddress + "/HoaDon/GetAll");

            if (response.IsSuccessStatusCode)
            {
                string data = await response.Content.ReadAsStringAsync();
                hoadon = JsonConvert.DeserializeObject<List<HoaDonViewModel>>(data);
            }
            if (maTrangThai.HasValue)
            {
                hoadon = hoadon.Where(h => h.MaTrangThai == maTrangThai.Value).ToList();
            }

            // Apply sorting
            switch (sortOrder)
            {
                case "date_desc":
                    hoadon = hoadon.OrderByDescending(h => h.NgayDat).ToList();
                    break;
                default: // Date ascending
                    hoadon = hoadon.OrderBy(h => h.NgayDat).ToList();
                    break;
            }

            int totalItems = hoadon.Count();
            var paginatedList = PaginatedList<HoaDonViewModel>.CreateAsync(hoadon.AsQueryable(), page ?? 1, pagesize ?? 9);

            ViewBag.Page = page;
            ViewBag.TotalPages = paginatedList.TotalPages;

            return View(paginatedList);
        }
        public async Task<IActionResult> GetDataSellProduct()
        {
            // Make the API call to get data
            var DataSellProduct = await _client.GetAsync(_client.BaseAddress + "/ThongKe/DataSellProduct");
            if (DataSellProduct.IsSuccessStatusCode)
            {
                // Read and deserialize the response
                string datasell = await DataSellProduct.Content.ReadAsStringAsync();
                var statics = JsonConvert.DeserializeObject<List<DataSellProduct>>(datasell);

                // Pass data to ViewBag
                ViewBag.DataSellProduct = statics;
            }
            else
            {
                // Handle error response or set default data
                ViewBag.DataSellProduct = new List<DataSellProduct>();
                ViewBag.ErrorMessage = "Unable to retrieve sales data.";
            }

            return View();
        }

        private List<int> GetCategoryData(List<CategorySalesStatistics> statistics, int categoryId, List<int> months)
        {
            return months.Select(month =>
            {
                var statForMonth = statistics.FirstOrDefault(s =>
                    s.Month == month && s.DanhMucId == categoryId);
                return statForMonth?.TotalQuantitySold ?? 0;
            }).ToList();
        }

        [Authorize]
        [HttpGet]
        [Route("Edit/{id}")]
        public IActionResult Edit(int id, string returnUrl = null)
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

                // If returnUrl is not provided, capture the Referer header
                if (string.IsNullOrEmpty(returnUrl) && Request.Headers.ContainsKey("Referer"))
                {
                    returnUrl = Request.Headers["Referer"].ToString();
                }

                // Store returnUrl in ViewBag and TempData for use in the view and post action
                ViewBag.ReturnUrl = returnUrl;
                TempData["ReturnUrl"] = returnUrl;

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
        [Route("Edit/{MaHD}")]
        public async Task<IActionResult> Edit(HoaDonViewModel model, int MaHD, string returnUrl = null)
        {
            try
            {
                model.ChiTietHds = model.ChiTietHds.Where(item => item.IsDeleted == false).ToList();

                string data = JsonConvert.SerializeObject(model);
                StringContent content = new StringContent(data, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await _client.PutAsync(_client.BaseAddress + "/HoaDon/Update/" + MaHD, content);

                if (response.IsSuccessStatusCode)
                {
                    TempData["success"] = "Hóa đơn đã được cập nhật!";

                    // Try to get returnUrl from various sources
                    if (string.IsNullOrEmpty(returnUrl) && TempData.ContainsKey("ReturnUrl"))
                    {
                        returnUrl = TempData["ReturnUrl"]?.ToString();
                    }

                    // Ensure we have a valid URL to return to
                    if (!string.IsNullOrEmpty(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }

                    return RedirectToAction("Index");
                }

                string errorContent = await response.Content.ReadAsStringAsync();
                TempData["error"] = "Cập nhật thất bại: " + errorContent;
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
        public async Task<IActionResult> Search(string? query, int? page, int? pagesize, int? maTrangThai, string sortOrder)
        {
            // Initialize default values
            if (page == null) page = 1;
            if (pagesize == null) pagesize = 7;

            // Set ViewBag values for maintaining state
            ViewBag.PageSize = pagesize;
            ViewBag.CurrentSort = sortOrder;
            ViewBag.DateSortParm = string.IsNullOrEmpty(sortOrder) ? "date_desc" : "";
            ViewBag.MaTrangThai = maTrangThai;
            ViewBag.CurrentQuery = query;

            // Get order statuses for dropdown
            List<TrangThaiHoaDonVM> trangThaiList = new List<TrangThaiHoaDonVM>();
            HttpResponseMessage statusResponse = await _client.GetAsync(_client.BaseAddress + "/TrangThaiHd/GetAll");
            if (statusResponse.IsSuccessStatusCode)
            {
                string statusData = await statusResponse.Content.ReadAsStringAsync();
                trangThaiList = JsonConvert.DeserializeObject<List<TrangThaiHoaDonVM>>(statusData);
            }
            ViewBag.TrangThaiHoaDon = trangThaiList;

            // Get all orders
            List<HoaDonViewModel> hoadon = new List<HoaDonViewModel>();
            HttpResponseMessage response = await _client.GetAsync(_client.BaseAddress + "/HoaDon/GetAll");
            if (response.IsSuccessStatusCode)
            {
                string data = await response.Content.ReadAsStringAsync();
                hoadon = JsonConvert.DeserializeObject<List<HoaDonViewModel>>(data);
            }
            else
            {
                return View("Error");
            }

            // Apply search filter if query exists
            if (!string.IsNullOrEmpty(query))
            {
                hoadon = hoadon.Where(h =>
                    h.HoTen.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                    h.MaKH.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                    h.DienThoai.Contains(query, StringComparison.OrdinalIgnoreCase)
                ).ToList();
            }

            // Filter by status if specified
            if (maTrangThai.HasValue)
            {
                hoadon = hoadon.Where(h => h.MaTrangThai == maTrangThai.Value).ToList();
            }

            // Apply sorting
            switch (sortOrder)
            {
                case "date_desc":
                    hoadon = hoadon.OrderByDescending(h => h.NgayDat).ToList();
                    break;
                default: // Date ascending
                    hoadon = hoadon.OrderBy(h => h.NgayDat).ToList();
                    break;
            }

            // Create paginated list
            var paginatedList = PaginatedList<HoaDonViewModel>.CreateAsync(
                hoadon.AsQueryable(),
                page ?? 1,
                pagesize ?? 7
            );

            // Set additional ViewBag properties for pagination
            ViewBag.Page = page;
            ViewBag.TotalPages = paginatedList.TotalPages;

            return View("ShowAll", paginatedList);
        }
        [Authorize]
        [HttpPost, ActionName("Delete")]
        public IActionResult DeleteConfirm(int MaHD, string returnUrl = null)
        {
            try
            {
                // If returnUrl is not provided and there's a Referer header, use it
                if (string.IsNullOrEmpty(returnUrl) && Request.Headers.ContainsKey("Referer"))
                {
                    returnUrl = Request.Headers["Referer"].ToString();
                }

                HttpResponseMessage response = _client.DeleteAsync(_client.BaseAddress + "/HoaDon/Delete/" + MaHD).Result;
                if (response.IsSuccessStatusCode)
                {
                    TempData["success"] = "Xóa thành công!";

                    // Return to the provided URL if available
                    if (!string.IsNullOrEmpty(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }

                    return RedirectToAction("Index");
                }

                TempData["error"] = "Xóa thất bại!";

                // If there's a returnUrl, redirect back to it
                if (!string.IsNullOrEmpty(returnUrl))
                {
                    return Redirect(returnUrl);
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;

                // If there's a returnUrl, redirect back to it
                if (!string.IsNullOrEmpty(returnUrl))
                {
                    return Redirect(returnUrl);
                }

                return RedirectToAction("Index");
            }
        }
    }
}