using System.Net.Sockets;
using Core.Service;
using Infrastructure.Client;
using Infrastructure.Dto;

namespace Core
{
    public class TwitchBot : ITwitchBotService
    {
        private readonly string clientId;
        private readonly string clientSecret;
        private readonly string botUsername;
        private readonly string redirectUri;
        private readonly string authorizationCode;
        public string ChannelId { get; set; }
        public string AccessToken { get; private set; }
        public string RefreshToken { get; private set; }
        private readonly TwitchApiClient twitchApiClient;
        private readonly TwitchChatClient twitchChatClient;

        private bool isConnected;

        public TwitchBot(TwitchSettings settings, TwitchApiClient twitchApiClient, TwitchChatClient twitchChatClient)
        {
            clientId = settings.ClientId;
            clientSecret = settings.ClientSecret;
            botUsername = settings.BotUsername;
            redirectUri = settings.RedirectUri;
            authorizationCode = settings.AuthorizationCode;
            ChannelId = settings.ChannelId;
            RefreshToken = settings.RefreshToken;
            this.twitchApiClient = twitchApiClient;
            this.twitchChatClient = twitchChatClient;
            isConnected = false;
        }

        public async Task SetUserAccessToken()
        {
            AccessToken =
                await twitchApiClient.GetUserAccessTokenAsync(clientId, clientSecret, authorizationCode, redirectUri);
        }

        public async Task RefreshUserAccessToken()
        {
            AccessToken = await twitchApiClient.RefreshUserAccessTokenAsync(clientId, clientSecret, RefreshToken);
        }

        public async Task СonnectToChat()
        {
            if (ChannelId == null)
            {
                throw new InvalidOperationException("Channel info is not set.");
            }
            await twitchChatClient.ConnectToTwitchAsync();
            isConnected = await twitchChatClient.AuthenticateAndJoinChannelAsync(AccessToken, botUsername, ChannelId);
            _ = Task.Run(() => twitchChatClient.MonitorChatAsync());
            while (true)
            {
                // Keep the application running
            }
        }
    }
}