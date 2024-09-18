using Core;
using Core.Service;
using Infrastructure.Client;
using Infrastructure.Dto;
using MessageForwarder.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MessageForwarder
{
    class Program
{
    static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        ConfigureServices(builder.Services); // Вынесите настройку сервисов сюда
        
        var app = builder.Build();
        
        Configure(app);
        
        app.Run();
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        services.AddControllersWithViews();
        services.AddSingleton<MyDbContext>();
        services.AddSingleton<BadWordsRepository>();
        
        File.Copy("appsettings.json", "bin/Debug/net8.0/appsettings.json", true);
        
        // Считывание конфигурации
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        // Регистрация зависимостей
        var twitchSettings = configuration.GetSection("TwitchSettings").Get<TwitchSettings>();
        var vkPlaySettings = configuration.GetSection("VKPlaySettings").Get<VKPlaySettings>();
    
        services.AddTransient<HttpClient>();
    
        services.AddSingleton(twitchSettings);
        services.AddSingleton(vkPlaySettings);
    
        services.AddSingleton<TwitchApiClient>();
        services.AddSingleton<TwitchChatClient>();
        services.AddSingleton<TwitchBot>();
    
        services.AddSingleton<VKPlayApiClient>();
        services.AddSingleton<VKPlayBot>();
    
        services.AddSingleton<BotsManager>();
    }


    private static void Configure(WebApplication app)
    {
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Home/Error");
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();
        app.UseRouting();
        app.UseAuthorization();

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");
    }
}

}
