using System.Net.Sockets;
using Core.Domain;
using Core.Service;
using Infrastructure.Client;
using Infrastructure.Dto;
using MessageForwarder.Data;

namespace Core
{
    public class TwitchBot
    {
        private readonly string clientId;
        private readonly string clientSecret;
        private readonly string botUsername;
        private readonly string redirectUri;
        private readonly string authorizationCode;
        public string Channel { get; set; }
        public string AccessToken { get; private set; }
        public string RefreshToken { get; private set; }
        private readonly TwitchApiClient twitchApiClient;
        private readonly TwitchChatClient twitchChatClient;
        private readonly BadWordsRepository badWordsRepository;
        public VKPlayBot VKPlayBot { get; set; } = null;

        public bool IsConnected { get; private set; }
        private bool tokenIsRefreshed;

        public TwitchBot(TwitchSettings settings, TwitchApiClient twitchApiClient, TwitchChatClient twitchChatClient,
            BadWordsRepository badWordsRepository)
        {
            clientId = settings.ClientId;
            clientSecret = settings.ClientSecret;
            botUsername = settings.BotUsername;
            redirectUri = settings.RedirectUri;
            authorizationCode = settings.AuthorizationCode;
            Channel = settings.Channel;
            RefreshToken = settings.RefreshToken;
            this.twitchApiClient = twitchApiClient;
            this.twitchChatClient = twitchChatClient;
            this.badWordsRepository = badWordsRepository;
            IsConnected = false;
            tokenIsRefreshed = false;
        }

        public async Task SetUserAccessToken()
        {
            AccessToken =
                await twitchApiClient.GetUserAccessTokenAsync(clientId, clientSecret, authorizationCode, redirectUri);
        }

        public void RefreshUserAccessToken()
        {
            if(tokenIsRefreshed)
                return;
            AccessToken = twitchApiClient.RefreshUserAccessTokenAsync(clientId, clientSecret, RefreshToken).Result;
            tokenIsRefreshed = true;
        }

        public async Task СonnectToChat(CancellationToken cancellationToken)
        {
            RefreshUserAccessToken();
            if (Channel == null)
                throw new Exception("Channel name is not initialized");
            if (VKPlayBot == null)
                throw new Exception("TwitchBot is not initialized");
            IsConnected = twitchChatClient.AuthenticateAndJoinChannelAsync(AccessToken, botUsername, Channel).Result;
            twitchChatClient.SendMessageAsync("TwitchBot bot is connected");
            while (IsConnected && !cancellationToken.IsCancellationRequested)
            {
                var message = await twitchChatClient.GetChatLastMessagesAsync(cancellationToken);
                if (message.AuthorNick == null)
                    continue;
                VKPlayBot.SendMessage(message);
            }
            IsConnected = false;
        }

        public void SendMessage(ChatMessage message)
        {
            if (!IsConnected)
                throw new Exception("TwitchBot is not connected");
            var words = message.Content.Split(' ');
            for (int i = 0; i < words.Length; i++)
            {
                string wordToCheck = words[i].ToLower(); 
                var badWord = badWordsRepository.GetBadWordByContent(wordToCheck);

                if (badWord != null)
                    words[i] = new string('*', words[i].Length);
            }
            string filteredContent = string.Join(" ", words);
            string messageStr = $"{message.AuthorNick}: {filteredContent}";
            twitchChatClient.SendMessageAsync(messageStr);
        }
    }
}