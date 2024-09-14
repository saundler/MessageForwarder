namespace Infrastructure.Dto;

public class VKPlaySettings
{
    public string Token { get; set; }
    public string Сhannel { get; set; }

    // Пустой конструктор для использования в различных сценариях, например, при десериализации
    public VKPlaySettings() { }

    // Конструктор для инициализации всех свойств
    public VKPlaySettings(string token, string channel)
    {
        Token = token;
        Сhannel = channel;
    }
}
