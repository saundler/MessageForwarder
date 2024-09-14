using Core.Domain;

namespace Core.Service;

public class BotsManager
{
    private readonly TwitchBot _twitchBot;
    private readonly VKPlayBot _vkPlayBot;
    private Task vkPlayBotTask;
    private Task twitchBotTask;
    private CancellationTokenSource vkPlayBotCts;
    private CancellationTokenSource twitchBotCts;

    public BotsManager(TwitchBot twitchBot, VKPlayBot vkPlayBot)
    {
        _twitchBot = twitchBot;
        _vkPlayBot = vkPlayBot;
        _vkPlayBot.TwitchBot = _twitchBot;
        _twitchBot.VKPlayBot = _vkPlayBot;
    }

    public void StartBots()
    {
        if (_twitchBot == null || _vkPlayBot == null)
            throw new Exception("Bots are not initalized");
        if (twitchBotTask == null || twitchBotTask.IsCompleted)
        {
            twitchBotCts = new CancellationTokenSource();
            twitchBotTask = Task.Run(() => _twitchBot.СonnectToChat(twitchBotCts.Token), twitchBotCts.Token);
        }
        if (vkPlayBotTask == null || vkPlayBotTask.IsCompleted)
        {
            vkPlayBotCts = new CancellationTokenSource();
            vkPlayBotTask = Task.Run(() => _vkPlayBot.СonnectToChat(vkPlayBotCts.Token), vkPlayBotCts.Token);
        }
    }

    public void StopBots()
    {
        if (vkPlayBotTask != null && !vkPlayBotTask.IsCompleted)
        {
            vkPlayBotCts.Cancel();
            try
            {
                vkPlayBotTask.Wait(); // Ждем завершения задачи
            }
            catch (AggregateException ex)
            {
                // Обрабатываем исключение отмены задачи
                if (ex.InnerExceptions[0] is OperationCanceledException)
                {
                    Console.WriteLine("VK Play Bot Task canceled.");
                }
                else
                {
                    throw;
                }
            }
        }
        if (twitchBotTask != null && !twitchBotTask.IsCompleted)
        {
            twitchBotCts.Cancel();
            try
            {
                twitchBotTask.Wait(); 
            }
            catch (AggregateException ex)
            {
                // Обрабатываем исключение отмены задачи
                if (ex.InnerExceptions[0] is OperationCanceledException)
                {
                    Console.WriteLine("Twitch Bot Task canceled.");
                }
                else
                {
                    throw;
                }
            }
        }
    }
    
    public bool AreBotsRunning()
    {
        return _twitchBot.IsConnected && _vkPlayBot.IsConnected;
    }

    public void UpdateBotsSettings(string TwitchChannel, string VkChannel)
    {
        _twitchBot.Channel = TwitchChannel;
        _vkPlayBot.Channel = VkChannel;
    }
}