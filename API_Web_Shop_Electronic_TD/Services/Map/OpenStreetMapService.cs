using API_Web_Shop_Electronic_TD.Models;
using Newtonsoft.Json;

namespace API_Web_Shop_Electronic_TD.Services.Map
{
    public class OpenStreetMapService
    {
        private readonly HttpClient _httpClient;

        public OpenStreetMapService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

		public async Task<List<AddressSuggestion>> GetAddressSuggestionsAsync(string query)
		{
			var url = $"https://nominatim.openstreetmap.org/search?q={Uri.EscapeDataString(query)}&format=json&addressdetails=1&limit=5";

			// Log the full URL
			Console.WriteLine($"Calling OpenStreetMap API: {url}");

			var request = new HttpRequestMessage(HttpMethod.Get, url);
			request.Headers.Add("User-Agent", "LocalHost (duong22tg@gmail.com)");

			// Log the request headers
			Console.WriteLine("Request Headers:");
			foreach (var header in request.Headers)
			{
				Console.WriteLine($"{header.Key}: {string.Join(", ", header.Value)}");
			}

			var response = await _httpClient.SendAsync(request);
			Console.WriteLine($"Response Status Code: {response.StatusCode}");

			if (response.IsSuccessStatusCode)
			{
				var json = await response.Content.ReadAsStringAsync();
				Console.WriteLine($"Response Content: {json}");

				var suggestions = JsonConvert.DeserializeObject<List<AddressSuggestion>>(json);
				Console.WriteLine($"Parsed {suggestions?.Count ?? 0} suggestions");
				return suggestions;
			}

			Console.WriteLine($"Error: {response.StatusCode}, {await response.Content.ReadAsStringAsync()}");
			return null;
		}
		public double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
		{
			const double R = 6371e3; // Bán kính Trái Đất tính bằng mét
			double phi1 = lat1 * Math.PI / 180; // Đổi độ sang radian
			double phi2 = lat2 * Math.PI / 180;
			double deltaPhi = (lat2 - lat1) * Math.PI / 180;
			double deltaLambda = (lon2 - lon1) * Math.PI / 180;

			double a = Math.Sin(deltaPhi / 2) * Math.Sin(deltaPhi / 2) +
			           Math.Cos(phi1) * Math.Cos(phi2) *
			           Math.Sin(deltaLambda / 2) * Math.Sin(deltaLambda / 2);
			double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

			return R * c; // Khoảng cách tính bằng mét
		}
		public bool IsInnerHanoi(AddressSuggestion address)
		{
			var innerDistricts = new List<string> { "Hoan Kiem", "Ba Dinh", "Dong Da", "Hai Ba Trung", "Cau Giay" };
			return innerDistricts.Any(district => address.display_name.Contains(district, StringComparison.OrdinalIgnoreCase));
		}

		public int CalculateShippingFee(AddressSuggestion address)
		{
			const double HanoiLat = 21.028511;
			const double HanoiLon = 105.804817;

			if (IsInnerHanoi(address))
			{
				return 10000; 
			}

			double distance = CalculateDistance(HanoiLat, HanoiLon, address.Lat, address.Lon) / 1000; // Đổi ra km

			if (distance <= 20)
			{
				return 20000; 
			}
			else
			{
				return 50000; 
			}
		}


	}
}
	