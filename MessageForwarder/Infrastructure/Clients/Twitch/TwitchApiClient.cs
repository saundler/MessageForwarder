using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Infrastructure.Client
{
    public class TwitchApiClient
    {
        private readonly HttpClient _httpClient;

        public TwitchApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string> GetUserAccessTokenAsync(string clientId, string clientSecret, string code, string redirectUri)
        {
            var requestData = new Dictionary<string, string>
            {
                { "client_id", clientId },
                { "client_secret", clientSecret },
                { "code", code },
                { "grant_type", "authorization_code" },
                { "redirect_uri", redirectUri }
            };

            var response = await _httpClient.PostAsync("https://id.twitch.tv/oauth2/token", new FormUrlEncodedContent(requestData));
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var jObject = JObject.Parse(responseContent);
                var accessToken = jObject["access_token"].ToString();
                var refreshToken = jObject["refresh_token"].ToString();
                Console.WriteLine($"Access Token: {accessToken}"); // Отладочная информация
                Console.WriteLine($"Refresh Token: {refreshToken}"); // Отладочная информация

                // Сохраните токены в надежном месте для использования в будущем
                return accessToken;
            }
            else
            {
                throw new Exception($"Failed to get access token: {response.StatusCode}, Response: {responseContent}");
            }
        }

        public async Task<string> RefreshUserAccessTokenAsync(string clientId, string clientSecret, string refreshToken)
        {
            var requestData = new Dictionary<string, string>
            {
                { "client_id", clientId },
                { "client_secret", clientSecret },
                { "grant_type", "refresh_token" },
                { "refresh_token", refreshToken }
            };

            var response = await _httpClient.PostAsync("https://id.twitch.tv/oauth2/token", new FormUrlEncodedContent(requestData));
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var jObject = JObject.Parse(responseContent);
                var accessToken = jObject["access_token"].ToString();
                var newRefreshToken = jObject["refresh_token"].ToString();
                Console.WriteLine($"New Access Token: {accessToken}"); // Отладочная информация
                Console.WriteLine($"New Refresh Token: {newRefreshToken}"); // Отладочная информация

                // Сохраните новый токен обновления в надежном месте для использования в будущем
                return accessToken;
            }
            else
            {
                throw new Exception($"Failed to refresh access token: {response.StatusCode}, Response: {responseContent}");
            }
        }
    }
}
