using System;
using System.Net.Http;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

public class TrovoChatClient
{
    private ClientWebSocket _clientWebSocket;

    public TrovoChatClient()
    {
        _clientWebSocket = new ClientWebSocket();
    }

    public async Task ConnectAsync(string token)
    {
        Uri serverUri = new Uri("wss://open-chat.trovo.live/chat");
        await _clientWebSocket.ConnectAsync(serverUri, CancellationToken.None);
        Console.WriteLine("WebSocket connection established");

        // Отправляем AUTH сообщение, используя только access_token
        var authMessage = new
        {
            type = "AUTH",
            nonce = Guid.NewGuid().ToString(),
            data = new
            {
                token = token // Здесь передаем только сам access_token
            }
        };

        string jsonMessage = JsonConvert.SerializeObject(authMessage);
        await SendMessageAsync(jsonMessage);

        // Запуск получателя сообщений
        _ = Task.Run(async () => await ReceiveMessagesAsync());

        // Начинаем цикл PING для поддержания соединения
        _ = Task.Run(async () => await StartPingLoop());
    }


    private async Task SendMessageAsync(string message)
    {
        try
        {
            byte[] bytesToSend = Encoding.UTF8.GetBytes(message);
            await _clientWebSocket.SendAsync(new ArraySegment<byte>(bytesToSend), WebSocketMessageType.Text, true, CancellationToken.None);
            Console.WriteLine("Message sent: " + message);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при отправке сообщения: {ex.Message}");
        }
    }

    private async Task ReceiveMessagesAsync()
    {
        byte[] buffer = new byte[1024 * 4];
        try
        {
            while (_clientWebSocket.State == WebSocketState.Open)
            {
                WebSocketReceiveResult result = await _clientWebSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    Console.WriteLine("Сервер закрыл соединение.");
                    await _clientWebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Соединение закрыто сервером", CancellationToken.None);
                }
                else
                {
                    string message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    Console.WriteLine("Received message: " + message);

                    // Добавьте логику обработки сообщений здесь
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при получении сообщения: {ex.Message}");
        }
    }

    // Метод для поддержания соединения через PING
    public async Task StartPingLoop()
    {
        try
        {
            while (_clientWebSocket.State == WebSocketState.Open)
            {
                var pingMessage = new
                {
                    type = "PING",
                    nonce = Guid.NewGuid().ToString()
                };

                string jsonMessage = JsonConvert.SerializeObject(pingMessage);
                await SendMessageAsync(jsonMessage);
                await Task.Delay(30000);  // Отправляем PING каждые 30 секунд
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка в PING цикле: {ex.Message}");
        }
    }

    public async Task DisconnectAsync()
    {
        try
        {
            if (_clientWebSocket.State == WebSocketState.Open)
            {
                await _clientWebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Disconnecting", CancellationToken.None);
                Console.WriteLine("WebSocket connection closed");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при отключении: {ex.Message}");
        }
    }
}