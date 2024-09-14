using Core.Domain;
using Infrastructure.Dto;
using MessageForwarder.Data;

namespace Core;

public class VKPlayBot
{
    private string authorId;
    private long LastReadingMessageTime; 
    public string Channel { get; set; }
    public string Token { get; set; }
    public TwitchBot TwitchBot { get; set; }

    private readonly VKPlayApiClient vkPlayApiClient;
    private readonly BadWordsRepository badWordsRepository;

    public bool IsConnected { get; private set; }

    public VKPlayBot(VKPlaySettings settings, VKPlayApiClient vkPlayApiClient, BadWordsRepository badWordsRepository)
    {
        Channel = settings.Сhannel;
        Token = settings.Token;
        this.vkPlayApiClient = vkPlayApiClient;
        this.badWordsRepository = badWordsRepository;
        IsConnected = false;
    }

    public void СonnectToChat(CancellationToken cancellationToken)
    {
        IsConnected = true;
        if(Channel == null) 
            throw new Exception("Channel name is not initialized");
        if (TwitchBot == null)
            throw new Exception("TwitchBot is not initialized");
        vkPlayApiClient.SendMessageAsync("VKPlay bot is connected", Channel, Token);
        ChatMessage message = new ChatMessage();
        while (message.Content != "VKPlay bot is connected")
        {
            message = vkPlayApiClient.GetChatLastMessagesAsync(Channel, Token).Result;
            authorId = message.AuthorId;
        }

        while (IsConnected && !cancellationToken.IsCancellationRequested)
        {
            message = vkPlayApiClient.GetChatLastMessagesAsync(Channel, Token).Result;
            if(message.CreatedAt <= LastReadingMessageTime || authorId == message.AuthorId)
                continue;
            LastReadingMessageTime = message.CreatedAt;
            TwitchBot.SendMessage(message);
        }
        IsConnected = false;
    }
    
    public async void SendMessage(ChatMessage message)
    {
        if (!IsConnected)
            throw new Exception("VKplayBot is not connected");
        var content = message.Content;
        content = content.Length > 2 ? content.Substring(0, content.Length - 2) : "";
        var words = content.Split(' ');
        for (int i = 0; i < words.Length; i++)
        {
            string wordToCheck = words[i].ToLower(); 
            var badWord = badWordsRepository.GetBadWordByContent(wordToCheck);

            if (badWord != null)
                words[i] = new string('*', words[i].Length);
        }
        string filteredContent = string.Join(" ", words);
        string messageStr = $"{message.AuthorNick}: {filteredContent}";
        await vkPlayApiClient.SendMessageAsync(messageStr, Channel, Token);
    }
}