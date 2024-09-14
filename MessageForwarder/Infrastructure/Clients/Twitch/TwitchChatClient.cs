using System.Net.WebSockets;
using System.Text;
using Core.Domain;
using Core.Service;

public class TwitchChatClient
{
    private ClientWebSocket client;
    private string channel;

    public TwitchChatClient()
    {
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

        var message = await GetChatLastMessagesAsync(CancellationToken.None);
        if (message.Content.Contains("Login authentication failed"))
        {
            Console.WriteLine("Login authentication failed.");
            return false;
        }

        Console.WriteLine("Login authentication successful.");
        this.channel = channel;
        return true;
    }

    public async Task SendMessageAsync(string message)
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

    public async Task<ChatMessage> GetChatLastMessagesAsync(CancellationToken cancellationToken)
    {
        if (client.State != WebSocketState.Open)
        {
            throw new InvalidOperationException("WebSocket is not connected.");
        }

        var buffer = new byte[1024 * 4];
        try
        {
            var result = await client.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);
            var content = Encoding.UTF8.GetString(buffer, 0, result.Count);
            var message = IChatMessageParser.ParseTwitcChatMessage(content);
            if (message == null)
                message = new ChatMessage(content);
            return message;
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"Error getting chat messages: {ex.Message}");
            throw;
        }
    }
}