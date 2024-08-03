using Core.Domain;

namespace Core.Service;

public interface IChatMessageParser
{
    public ChatMessage ParseTwitcChatMessage(string rawMessage);
}