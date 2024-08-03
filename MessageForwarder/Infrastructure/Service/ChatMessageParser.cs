using Core.Domain;
using Core.Service;

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
}
