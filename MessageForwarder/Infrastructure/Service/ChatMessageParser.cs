using Core.Domain;
using Newtonsoft.Json.Linq;
using IChatMessageParser = Core.Service.IChatMessageParser;

namespace Infrastructure.Service;
public class ChatMessageParser : IChatMessageParser
{
    public ChatMessage ParseTwitcChatMessage(string rawMessage)
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
                        chatMessage.UserId = value;
                        break;
                    case "display-name":
                        chatMessage.DisplayName = value;
                        break;
                }
            }

            // Parse message
            chatMessage.Message = parts[4].Substring(1);

            return chatMessage;
        }
        return null;
    }
    
    public ChatMessage ParseVKLiveChatMessage(string rawMessage)
    {
        // Используем JObject для динамического разбора JSON
        var jObject = JObject.Parse(rawMessage);

        // Создаем экземпляр ChatMessage и заполняем его данными из JSON
        var chatMessage = new ChatMessage
        {
            UserId = jObject["Result"]?["Data"]?["Data"]?["Data"]?["Author"]?["Id"]?.ToString(),
            DisplayName = jObject["Result"]?["Data"]?["Data"]?["Data"]?["Author"]?["DisplayName"]?.ToString(),
            Message = ParseMessageContent(jObject["Result"]?["Data"]?["Data"]?["Data"]?["Data"])
        };

        return chatMessage;
    }
    
    private static string ParseMessageContent(JToken messageContentToken)
    {
        if (messageContentToken == null) return string.Empty;

        var messageContent = string.Empty;
        foreach (var content in messageContentToken)
        {
            if (content["Type"]?.ToString() == "text")
            {
                messageContent += content["Content"]?.ToString();
            }
            else if (content["Type"]?.ToString() == "smile")
            {
                // Добавляем поддержку эмодзи или смайликов
                messageContent += $"[{content["Name"]}]";
            }
        }
        return messageContent;
    }
}
