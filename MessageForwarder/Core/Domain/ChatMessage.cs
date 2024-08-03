namespace Core.Domain;

public class ChatMessage
{
    public string UserId { get; set; }
    public string DisplayName { get; set; }
    public string Message { get; set; }
}