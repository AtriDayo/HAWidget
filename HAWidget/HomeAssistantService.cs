using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace HAWidget
{
    public class HomeAssistantService
    {
        private readonly string _baseUrl;
        private readonly string _token;

        public HomeAssistantService(string baseUrl, string token)
        {
            _baseUrl = baseUrl?.TrimEnd('/') ?? "";
            _token = token ?? "";
        }

        public async Task<string> GetEntityStateAsync(string entityId)
        {
            string url = $"{_baseUrl}/api/states/{entityId}";

            using HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + _token);

            var response = await client.GetAsync(url);
            string json = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("HTTP状态码: " + response.StatusCode + "\n返回内容:\n" + json);
            }

            using JsonDocument doc = JsonDocument.Parse(json);
            string? state = doc.RootElement.GetProperty("state").GetString();

            return state ?? "--";
        }

        public async Task CallServiceAsync(string domain, string service, object data)
        {
            string url = $"{_baseUrl}/api/services/{domain}/{service}";
            string json = JsonSerializer.Serialize(data);

            using HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + _token);

            using StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.PostAsync(url, content);
            string responseText = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("HTTP状态码: " + response.StatusCode + "\n返回内容:\n" + responseText);
            }
        }

        public async Task TurnLightOnAsync(string entityId)
        {
            await CallServiceAsync("light", "turn_on", new
            {
                entity_id = entityId
            });
        }

        public async Task TurnLightOffAsync(string entityId)
        {
            await CallServiceAsync("light", "turn_off", new
            {
                entity_id = entityId
            });
        }
    }
}