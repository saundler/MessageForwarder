using System.Net.WebSockets;
using System.Text;
using Newtonsoft.Json;

public class WebSocketClient
{
    private readonly string wsConnectionAddr = "wss://pubsub.live.vkplay.ru/connection/websocket";
    private readonly string wsTokenURL = "https://api.live.vkplay.ru/v1/ws/connect";
    private readonly string originURL = "https://live.vkplay.ru";
    
    private ClientWebSocket _webSocket;
    private readonly HttpClient _httpClient;
    private string _channel;
    private string token;
    private string wsChannel;
    
    public class TokenResponse
    {
        public string Token { get; set; }
    }
    public class BlogResponse
    {
        public string PublicWebSocketChannel { get; set; }
        public string BlogUrl { get; set; }
    }
    
    public WebSocketClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _channel = "saunder";
    }
    
    public struct MessageBlock
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("content")]
        public string Content { get; set; }

        [JsonProperty("modificator", NullValueHandling = NullValueHandling.Ignore)]
        public string Modificator { get; set; }

        [JsonProperty("url", NullValueHandling = NullValueHandling.Ignore)]
        public string URL { get; set; }

        [JsonProperty("explicit", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Explicit { get; set; }
    }
    
    public async Task<string> GetWebSocketTokenAsync()
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, wsTokenURL);
            request.Headers.Add("X-From-Id", Guid.NewGuid().ToString());

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            var tokenResponse = JsonConvert.DeserializeObject<TokenResponse>(responseContent);

            if (tokenResponse?.Token == null)
            {
                throw new Exception("Token not found in the response.");
            }

            return tokenResponse.Token;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GetWebSocketTokenAsync: {ex.Message}");
            throw;
        }
    }
    
    private async Task SendInitializationMessageAsync()
    {
        var initMessage = new
        {
            id = 1,
            @params = new
            {
                token = token,
                name = "js",
            }
        };

        await SendMessageAsync(initMessage);
    }
    
    public async Task<BlogResponse> GetBlogAsync(string channel)
    {
        try
        {
            var blogUrl = $"https://api.live.vkplay.ru/v1/blog/{channel}";
            var response = await _httpClient.GetAsync(blogUrl);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            var blogResponse = JsonConvert.DeserializeObject<BlogResponse>(responseContent);

            if (blogResponse == null)
            {
                throw new Exception("Failed to retrieve blog information.");
            }

            return blogResponse;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GetBlogAsync: {ex.Message}");
            throw;
        }
    }
    
    private async Task ConnectToChatAsync(string wsChannel)
    {
        string jsonPayload = $@"
        {{
          ""id"": 0,
          ""params"": {{
            ""channel"": ""public-chat:{wsChannel}""
          }},
          ""method"": 1
        }}";
        await SendMessageAsync(jsonPayload);
    }
    
    public async Task ReceiveMessagesAsync()
    {
        var buffer = new byte[1024 * 4];
        while (_webSocket.State == WebSocketState.Open)
        {
            var result = await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            
            if (result.MessageType == WebSocketMessageType.Text)
            {
                var messageString = Encoding.UTF8.GetString(buffer, 0, result.Count);
                Console.WriteLine("Received VK: " + messageString);
            } 
            else if (result.MessageType == WebSocketMessageType.Close)
            {
                Console.WriteLine("Received VK:" + _webSocket.CloseStatus + " " + _webSocket.CloseStatusDescription);
                await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
            }
            else
            {
                Console.WriteLine("ОТВЕТ ПРИШЕЛ");
            }
        }
    }
    
    public async Task ConnectAsync()
    {
        try
        {
            _webSocket = new ClientWebSocket();
            _webSocket.Options.SetRequestHeader("Origin", originURL);
            await _webSocket.ConnectAsync(new Uri(wsConnectionAddr), CancellationToken.None);

            token = await GetWebSocketTokenAsync();
            Console.WriteLine($"Received token: {token}");
            
            await SendInitializationMessageAsync();
            
            var blogResponse = await GetBlogAsync(_channel);
            Console.WriteLine($"Blog channel: {blogResponse.PublicWebSocketChannel}");
            
            wsChannel = blogResponse.PublicWebSocketChannel.Split(':')[1];
            Console.WriteLine($"Got WSChannel:, {wsChannel}");
            
            await ConnectToChatAsync(wsChannel);
            
            // Start receiving messages
            await ReceiveMessagesAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in ConnectAsync: {ex.Message}");
            throw;
        }
    }
    
    
    
    
    public async Task SendMessageAsync(string message)
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
        var url = $"https://api.live.vkplay.ru/v1/blog/{_channel}/public_video_stream/chat";
        var request = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = content
        };

        // Добавление заголовков
        request.Headers.Add("Authorization", "Bearer " + "04b920b76736e4de56a9b7c2fe1797c0e4c9c2124fa073afec3e212383608d24");

        // Отправка запроса и получение ответа
        try
        {
            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode(); // Бросает исключение, если код статуса не успешный

            Console.WriteLine($"Got response from vk: {response.Headers}");
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"Error sending message: {ex.Message}");
            throw;
        }
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
    
    private async Task SendMessageAsync(object message)
    {
        var messageString = JsonConvert.SerializeObject(message);
        var buffer = Encoding.UTF8.GetBytes(messageString);
        await _webSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
    }
}


