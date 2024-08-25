using System;
using System.IO;
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
            // var serviceCollection = new ServiceCollection();
            // ConfigureServices(serviceCollection);
            //
            // var serviceProvider = serviceCollection.BuildServiceProvider();
            //
            // var twitchBot = serviceProvider.GetService<TwitchBot>();
            // var vkLiveBot = serviceProvider.GetService<VKLiveBot>();
            // var chatBot = serviceProvider.GetService<ChatBot>();
            //
            // // await vkLiveBot.СonnectToChat();
            // // await twitchBot.RefreshUserAccessToken();
            // // await twitchBot.СonnectToChat();
            //
            // // Пример отправки сообщения с помощью chatBot
            // await chatBot.SendMessageAsync("saunder", "Hello, World!");

            // string clientId = "60a261b04b75fd65fc022ec4c240e6d1"; // Замените на ваш client_id
            // string clientSecret = "49158d9752c4baa71277a4ff3a2b4f55"; // Замените на ваш client_secret
            // string authorizationCode = "YOUR_AUTHORIZATION_CODE"; // Код авторизации, полученный через OAuth 2.0
            // string redirectUri = "http://localhost:3000"; // Ваш redirect URI

            using (HttpClient httpClient = new HttpClient())
            {
                // Создаем экземпляр WebSocketClient
                WebSocketClient webSocketClient = new WebSocketClient(httpClient);

                // Подключаемся к WebSocket серверу
                await webSocketClient.ConnectAsync();

                // // Отправляем сообщение в чат VK Play Live
                string messageToSend = "Привет, Актан!";
                await webSocketClient.SendMessageAsync(messageToSend);
                // while (true)
                // {
                    // await webSocketClient.ReceiveMessagesAsync();
                // }
            }
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            if (!File.Exists("appsettings.json"))
            {
                File.Copy("../../../appsettings.json", "appsettings.json", true);
            }

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
            services.AddTransient<WebSocketClient>(sp => new WebSocketClient(sp.GetRequiredService<HttpClient>()));
        }
    }
}
