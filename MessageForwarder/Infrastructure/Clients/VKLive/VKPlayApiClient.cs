using System.Net.Http.Headers;
using Core.Domain;
using Core.Service;
using Newtonsoft.Json;

public struct MessageBlock
{
    [JsonProperty("type")] public string Type { get; set; }

    [JsonProperty("content")] public string Content { get; set; }

    [JsonProperty("modificator", NullValueHandling = NullValueHandling.Ignore)]
    public string Modificator { get; set; }

    [JsonProperty("url", NullValueHandling = NullValueHandling.Ignore)]
    public string URL { get; set; }

    [JsonProperty("explicit", NullValueHandling = NullValueHandling.Ignore)]
    public bool? Explicit { get; set; }
}

public class VKPlayApiClient
{
    private readonly string originURL = "https://live.vkplay.ru";
    private readonly string wsConnectionAddr = "wss://pubsub.live.vkplay.ru/connection/websocket";

    private readonly HttpClient httpClient;

    public VKPlayApiClient(HttpClient httpClient)
    {
        this.httpClient = httpClient;
    }

    // Метод для сериализации сообщения с использованием структуры MessageBlock
    private List<MessageBlock> SerializeMessage(string message)
    {
        var messageBlocks = new List<MessageBlock>();
        if (!string.IsNullOrEmpty(message))
        {
            messageBlocks.Add(new MessageBlock
            {
                Type = "text",
                Content = $"[\"{message}\",\"unstyled\",[]]"
            });
        }

        return messageBlocks;
    }

    public async Task<bool> SendMessageAsync(string message, string Сhannel, string Token)
    {
        // Сериализация сообщения с использованием структуры MessageBlock
        var serializedMessage = SerializeMessage(message);
        var serializedMessageJSON = JsonConvert.SerializeObject(serializedMessage);

        // Создание тела запроса
        var content = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("data", serializedMessageJSON)
        });

        // Создание URL и запроса
        var url = $"https://api.live.vkplay.ru/v1/blog/{Сhannel}/public_video_stream/chat";
        var request = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = content
        };

        // Добавление заголовков
        request.Headers.Add("Authorization", "Bearer " + Token);

        // Отправка запроса и получение ответа
        try
        {
            var response = await httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode(); // Бросает исключение, если код статуса не успешный

            Console.WriteLine($"Got response from vk: {response.Headers}");
            return true;
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"Error sending message: {ex.Message}");
            return false;
        }
    }

    public async Task<ChatMessage> GetChatLastMessagesAsync(string Сhannel, string Token)
    {
        // Создание URL запроса
        var url = $"https://api.live.vkplay.ru/v1/blog/{Сhannel}/public_video_stream/chat";

        // Создание запроса
        var request = new HttpRequestMessage(HttpMethod.Get, url);

        // Добавление заголовков
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", Token);

        // Отправка запроса и получение ответа
        try
        {
            var response = await httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode(); // Бросает исключение, если код статуса не успешный

            var content = await response.Content.ReadAsStringAsync();
            var message = IChatMessageParser.ParseVKLiveChatMessage(content);
            return message;
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"Error getting chat messages: {ex.Message}");
            throw;
        }
    }
}