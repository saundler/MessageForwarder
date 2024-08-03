using System;
using System.Net.Http;
using System.Threading.Tasks;
using Core;
using Core.Service;
using Infrastructure.Client;
using Infrastructure.Dto;
using Infrastructure.Service;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MessageForwarder
{
    class Program
    {
        static async Task Main(string[] args)
        {
            
            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);

            var serviceProvider = serviceCollection.BuildServiceProvider();

            var twitchBot = serviceProvider.GetService<TwitchBot>();

            await twitchBot.RefreshUserAccessToken();
            await twitchBot.СonnectToChat();
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            File.Copy("../../../appsettings.json", "appsettings.json", true);
            // Настройка конфигурации
            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            // Считывание настроек
            var twitchSettings = configuration.GetSection("TwitchSettings").Get<TwitchSettings>();

            // Регистрация зависимостей
            services.AddSingleton(twitchSettings);
            services.AddSingleton<HttpClient>();
            services.AddTransient<IChatMessageParser, ChatMessageParser>();
            services.AddTransient<TwitchApiClient>();
            services.AddTransient<TwitchChatClient>();
            services.AddTransient<TwitchBot>();
        }
    }
}