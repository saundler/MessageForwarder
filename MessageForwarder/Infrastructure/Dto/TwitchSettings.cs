namespace Infrastructure.Dto;

public class TwitchSettings
{
    public string ClientId { get; set; }
    public string ClientSecret { get; set; }
    public string BotUsername { get; set; }
    public string RedirectUri { get; set; }
    public string RefreshToken { get; set; }
    public string Channel { get; set; }
    public string AuthorizationCode { get; set; }

    // Пустой конструктор для использования в различных сценариях, например, при десериализации
    public TwitchSettings() { }

    // Конструктор для инициализации всех свойств
    public TwitchSettings(string clientId, string clientSecret, string botUsername, string redirectUri, string refreshToken, string channel, string authorizationCode)
    {
        ClientId = clientId;
        ClientSecret = clientSecret;
        BotUsername = botUsername;
        RedirectUri = redirectUri;
        RefreshToken = refreshToken;
        Channel = channel;
        AuthorizationCode = authorizationCode;
    }
}