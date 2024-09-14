using Core.Domain;
using Newtonsoft.Json.Linq;

namespace Core.Service;

public interface IChatMessageParser
{
    public static ChatMessage ParseTwitcChatMessage(string rawMessage)
    {
        if (rawMessage.Contains("PRIVMSG"))
        {
            var chatMessage = new ChatMessage();
            var parts = rawMessage.Split(new[] { ' ' }, 5);
        
            // Parse tags
            var tagsPart = parts[0];
            var tags = tagsPart.Substring(1).Split(';');
            foreach (var tag in tags)
            {
                var keyValue = tag.Split('=');
                var key = keyValue[0];
                var value = keyValue.Length > 1 ? keyValue[1] : string.Empty;
        
                switch (key)
                {
                    case "user-id":
                        chatMessage.AuthorId = value;
                        break;
                    case "display-name":
                        chatMessage.AuthorNick = value;
                        break;
                }
            }
        
            // Parse message
            chatMessage.Content = parts[4].Substring(1);
        
            return chatMessage;
        }
        return null;
    }
    
    public static ChatMessage ParseVKLiveChatMessage(string jsonResponse)
    {
        var jsonObject = JObject.Parse(jsonResponse);

        // Извлекаем сообщение
        string content = jsonObject["data"][0]["data"][0]["content"].ToString();
        
        // Преобразуем контент в нужный формат
        var contentArray = JArray.Parse(content);
        string messageContent = contentArray[0].ToString();

        // Извлекаем ник автора
        string authorNick = jsonObject["data"][0]["author"]["nick"].ToString();

        // Извлекаем ID автора
        string authorId = jsonObject["data"][0]["author"]["id"].ToString();

        // Извлекаем время отправки
        long createdAt = (long)jsonObject["data"][0]["createdAt"];

        return new ChatMessage
        {
            Content = messageContent,
            AuthorNick = authorNick,
            AuthorId = authorId, // Заполняем новое поле ID автора
            CreatedAt = createdAt
        };
    }
}