using System.Diagnostics;
using Core.Service;
using Microsoft.AspNetCore.Mvc;
using Forwarder.Models;
using Infrastructure.Dto;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SO=System.IO.File;

namespace Forwarder.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly TwitchSettings _twitchSettings;
    private readonly VKPlaySettings _vkPlaySettings;
    private readonly VKPlayApiClient _vkPlayApiClient;
    private readonly BotsManager _botsManager;

    public HomeController(ILogger<HomeController> logger, TwitchSettings twitchSettings, VKPlaySettings vkPlaySettings,
        BotsManager botsManager, VKPlayApiClient vkPlayApiClient)
    {
        _twitchSettings = twitchSettings;
        _vkPlaySettings = vkPlaySettings;
        _botsManager = botsManager;
        _vkPlayApiClient = vkPlayApiClient;
        _logger = logger;
    }

    public IActionResult Index()
    {
        // Получение текущих настроек ботов, если они сохранены
        var model = new BotSettingsViewModel
        {
            TwitchChannel = _twitchSettings.Channel,
            VkBotToken = _vkPlaySettings.Token,
            VkChannel = _vkPlaySettings.Сhannel,
            AreBotsEnabled = _botsManager.AreBotsRunning(), // Отображаем текущее состояние ботов
        };

        return View(model);
    }

    [HttpPost]
    public IActionResult Index(BotSettingsViewModel model, string action)
    {
        ModelState.Clear();
        if (ModelState.IsValid)
        {
            if (!_botsManager.AreBotsRunning())
            {
                _botsManager.UpdateBotsSettings(model.TwitchChannel, model.VkChannel);
        
                _twitchSettings.Channel = model.TwitchChannel;
                _vkPlaySettings.Token = model.VkBotToken;
                _vkPlaySettings.Сhannel = model.VkChannel;
                
                var json = SO.ReadAllText("appsettings.json");
                var jsonObj = JObject.Parse(json);

                jsonObj["TwitchSettings"] = JObject.FromObject(_twitchSettings);
                jsonObj["VKPlaySettings"] = JObject.FromObject(_vkPlaySettings);

                string output = JsonConvert.SerializeObject(jsonObj, Formatting.Indented);
                SO.WriteAllText("appsettings.json", output);
            }

            // Обработка действий включения/выключения ботов
            if (action == "toggleBots")
            {
                if (_botsManager.AreBotsRunning())
                {
                    StopBots(model);
                }
                else
                {
                    StartBots(model);
                }
            }
        }
        return View(model);
    }


    private void StartBots(BotSettingsViewModel model)
    {
        // Логика запуска ботов
        _logger.LogInformation(
            "Боты включены с настройками: Twitch Channel - {TwitchChannel}, VK Channel - {VkChannel}",
            model.TwitchChannel, model.VkChannel);
        if (!_vkPlayApiClient.SendMessageAsync("VKPlay bot check arguments", _vkPlaySettings.Сhannel,
                _vkPlaySettings.Token).Result)
        {
            ModelState.Clear();
            _logger.LogError("Ошибка при запуске ботов");
            ModelState.AddModelError(String.Empty,"Некорректное значение канала или токена, повторите попытку");
            StopBots(model);
        }
        else
        {
            _botsManager.StartBots();
            model.AreBotsEnabled = true;
        }
    }

    private void StopBots(BotSettingsViewModel model)
    {
        // Логика остановки ботов
        _logger.LogInformation("Боты отключены");
        _botsManager.StopBots();
        model.AreBotsEnabled = false;
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}