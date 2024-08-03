namespace Infrastructure.Dto;

public record TwitchSettings(
    string ClientId, 
    string ClientSecret, 
    string BotUsername, 
    string ChannelId, 
    string RedirectUri,
    string AuthorizationCode,
    string RefreshToken);