using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Specialized;
using System.Text;
using System.Web;
using Newtonsoft.Json.Linq; // Не забудьте установить пакет Newtonsoft.Json для работы с JSON

public class TrovoApiClient
{
    private static readonly string tokenUrl = "https://open-api.trovo.live/openplatform/exchangetoken";
    private readonly string clientId = "60a261b04b75fd65fc022ec4c240e6d1";
    private readonly string clientSecret = "49158d9752c4baa71277a4ff3a2b4f55";
    private readonly string redirectUri = "http://localhost:3000";
    
    // Генерация URL для авторизации
    private string GenerateAuthUrl()
    {
        var uriBuilder = new UriBuilder("https://open.trovo.live/page/login.html");
        var query = HttpUtility.ParseQueryString(uriBuilder.Query);
        query["client_id"] = clientId;
        query["response_type"] = "code";
        query["redirect_uri"] = redirectUri;
        query["scope"] = "chat_send_self user_details_self";
        uriBuilder.Query = query.ToString();
        return uriBuilder.ToString();
    }

    // Получение кода авторизации
    public async Task<string> GetAuthorizationCodeAsync()
    {
        string authUrl = GenerateAuthUrl();
        Console.WriteLine("Перейдите по этой ссылке и авторизуйтесь:");
        Console.WriteLine(authUrl);

        Console.WriteLine("Введите код авторизации:");
        return Console.ReadLine();
    }

    // Обмен кода на токен
    public async Task<string> ExchangeCodeForTokenAsync(string authorizationCode)
    {
        using (HttpClient client = new HttpClient())
        {
            // Создаем JSON тело запроса
            var jsonData = new
            {
                client_secret = clientSecret,
                grant_type = "authorization_code",
                code = authorizationCode,
                redirect_uri = redirectUri
            };

            // Преобразуем данные в JSON формат
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(jsonData);

            // Устанавливаем заголовки
            var request = new HttpRequestMessage(HttpMethod.Post, tokenUrl);
            request.Headers.Add("Accept", "application/json");
            request.Headers.Add("client-id", clientId);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                // Выполняем POST запрос
                HttpResponseMessage response = await client.SendAsync(request);
                string responseContent = await response.Content.ReadAsStringAsync();

                // Проверяем успешность ответа
                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("Токен получен успешно:");
                    Console.WriteLine(responseContent);
                    return responseContent;
                }
                else
                {
                    Console.WriteLine("Ошибка при обмене кода на токен:");
                    Console.WriteLine(responseContent);
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Произошла ошибка: {ex.Message}");
                return null;
            }
        }
    }

    // Извлечение access_token из ответа
    private string ExtractToken(string responseContent)
    {
        try
        {
            var jsonResponse = JObject.Parse(responseContent);
            string token = jsonResponse["access_token"]?.ToString();
            
            if (!string.IsNullOrEmpty(token))
            {
                return token;
            }
            else
            {
                Console.WriteLine("Не удалось найти поле access_token в ответе.");
                return null;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при обработке JSON: {ex.Message}");
            return null;
        }
    }
}
