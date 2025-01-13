using Newtonsoft.Json;
using TDProjectMVC.ViewModels;

namespace TDProjectMVC.Services.Map
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

            // Tạo yêu cầu HTTP với User-Agent
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("User-Agent", "LocalHost (duong22tg@gmail.com)");

            // Gửi yêu cầu
            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<AddressSuggestion>>(json);
            }

            Console.WriteLine($"Error: {response.StatusCode}, {await response.Content.ReadAsStringAsync()}");
            return null;
        }
    }
}
