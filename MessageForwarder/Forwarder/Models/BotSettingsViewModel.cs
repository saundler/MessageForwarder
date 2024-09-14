namespace Forwarder.Models
{
    public class BotSettingsViewModel
    {
        public string TwitchChannel { get; set; }
        public string VkBotToken { get; set; }
        public string VkChannel { get; set; }
        public bool AreBotsEnabled { get; set; }
    }
}