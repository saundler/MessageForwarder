namespace Core.Domain;

public class ChatMessage
{
    public string Content { get; set; }
    public string AuthorNick { get; set; }
    public long CreatedAt { get; set; }
    public string AuthorId { get; set; }

    public ChatMessage() {}

    public ChatMessage(string content)
    {
        Content = content;
    }
}