using System.Net.WebSockets;
using System.Text;
using Core.Service;

public class TwitchChatClient
{
    private ClientWebSocket client;
    private string channel;
    private readonly IChatMessageParser parser;

    public TwitchChatClient(IChatMessageParser parser)
    {
        this.parser = parser;
        channel = "NotSpecified";
    }

    private async Task ConnectAsync()
    {
        client = new ClientWebSocket();
        await client.ConnectAsync(new Uri("wss://irc-ws.chat.twitch.tv:443"), CancellationToken.None);
        Console.WriteLine("Connected to Twitch IRC");
    }
    
    private async Task SendAsync(string message)
    {
        if (client?.State != WebSocketState.Open)
        {
            throw new InvalidOperationException("WebSocket is not connected.");
        }
        var bytes = Encoding.UTF8.GetBytes(message + "\r\n");
        await client.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
    }
    
    public async Task<bool> AuthenticateAndJoinChannelAsync(string token, string username, string channel)
    {
        await ConnectAsync();
        await SendAsync($"PASS oauth:{token}");
        await SendAsync($"NICK {username}");
        await SendAsync("CAP REQ :twitch.tv/membership twitch.tv/tags twitch.tv/commands");
        await SendAsync($"JOIN #{channel}");

        var message = await GetMessageAsync();
        if (message.Contains("Login authentication failed"))
        {
            Console.WriteLine("Login authentication failed.");
            return false;
        }

        Console.WriteLine("Login authentication successful.");
        this.channel = channel;
        return true;
    }
    
    private async Task SendMessageAsync(string message)
    {
        if (client?.State != WebSocketState.Open)
        {
            throw new InvalidOperationException("WebSocket is not connected.");
        }
        if (channel == "NotSpecified")
        {
            throw new InvalidOperationException("Authentication and channel join failed.");
        }
        await SendAsync($"PRIVMSG #{channel} :{message}");
    }

    private async Task<string> GetMessageAsync()
    {
        if (client.State != WebSocketState.Open)
        {
            throw new InvalidOperationException("WebSocket is not connected.");
        }
        var buffer = new byte[1024 * 4];
        var result = await client.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
        var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
        return message;
    }

    public async Task MonitorChatAsync()
    {
        while (client.State == WebSocketState.Open)
        {
            var message = GetMessageAsync();

            var chatMessage = parser.ParseTwitcChatMessage(message.Result);
            if (chatMessage != null)
            {
                Console.WriteLine($"User ID: {chatMessage.UserId}");
                Console.WriteLine($"Display Name: {chatMessage.DisplayName}");
                Console.WriteLine($"Message: {chatMessage.Message}");
            }
        }
    }
}