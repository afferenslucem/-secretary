﻿using Secreatry.HealthCheck.Data;
using Secretary.Cache;
using Secretary.Configuration;
using Secretary.Logging;
using Secretary.Storage;
using Secretary.Storage.Models;
using Secretary.Telegram.chains;
using Secretary.Telegram.Commands;
using Secretary.Telegram.Commands.ExceptionHandlers;
using Secretary.Telegram.Commands.Executors;
using Secretary.Telegram.Exceptions;
using Secretary.Telegram.Sessions;
using Secretary.Yandex.Authentication;
using Secretary.Yandex.Mail;
using Serilog;

namespace Secretary.Telegram;

public class TelegramBot
{
    public static readonly string Version = "v3.0.1";
    
    public static readonly DateTime Uptime = DateTime.UtcNow;
    
    public CommandsListeningChain Chain;
    
    private ICacheService _cacheService;
    private ISessionStorage _sessionStorage;
    private Database _database;
    private ILogger _logger = LogPoint.GetLogger<TelegramBot>();
    private IYandexAuthenticator _yandexAuthenticator;
    private IMailClient _mailClient;
    private ITelegramClient _telegramClient { get; set; }
    public long ReceivedMessages { get; private set; } = 0;
    
    public TelegramBot (
        Database database,
        ICacheService redisCacheService,
        ISessionStorage sessionStorage,
        IYandexAuthenticator yandexAuthenticator,
        IMailClient mailClient,
        ITelegramClient telegramClient
    ) {
        _database = database;
        _cacheService = redisCacheService;
        _sessionStorage = sessionStorage;
        _yandexAuthenticator = yandexAuthenticator;
        _mailClient = mailClient;
        _telegramClient = telegramClient;
    }

    public void Init()
    {
        Chain = new CommandsListeningChain();

        _telegramClient.OnMessage += WorkWithMessage;
    }

    public async Task WorkWithMessage(BotMessage message)
    {
        var command = Chain.Get(message.Text)!;

        ReceivedMessages++;
        
        try
        {
            LogCommand(command, $"{message.ChatId}: execute command {command.GetType().Name}");

            var context = new CommandContext(
                message.ChatId,
                _telegramClient,
                _sessionStorage,
                _database.UserStorage,
                _database.DocumentStorage,
                _database.EmailStorage,
                _database.EventLogStorage,
                _yandexAuthenticator,
                _mailClient,
                _cacheService,
                message.Text
            );

            await new CommandExecutor(command, context).Execute();
            await new CommandExecutor(command, context).OnComplete();
        }
        catch (ForceCompleteCommandException e)
        {
            _logger.Error(e, $"{message.ChatId}: Сommand execution fault {command.GetType().Name}");
        }
        catch (Exception e)
        {
            await CatchException(e, message.ChatId);
            
            _logger.Error(e, $"{message.ChatId}: Сommand execution fault {command.GetType().Name}");
            await _sessionStorage.DeleteSession(message.ChatId);
        }
    }
    
    public Task Listen()
    {
        _logger.Information($"Version: {Version}");
        _logger.Information($"Uptime: {Uptime}");
        
        return _telegramClient.RunDriver();
    }

    public void LogCommand(Command command, string message)
    {
        if (command.GetType() == typeof(NullCommand)) return;
        
        _logger.Information(message);
    }

    private async Task CatchException(Exception e, long chatId)
    {
        if (e is NonCompleteUserException nonCompleteUserException)
        {
            await new NonCompleteUserExceptionHandler().Handle(nonCompleteUserException, chatId, _telegramClient);
        }
        else
        {
            await _telegramClient.SendMessage(chatId, "Произошла непредвиденная ошибка");
        }
    }

    public BotHealthData GetHealthData()
    {
        var healthData = new BotHealthData();
        
        healthData.Version = Version;
        healthData.DeployTime = Uptime;
        healthData.PingTime = _telegramClient.LastCheckTime;
        healthData.ReceivedMessages = ReceivedMessages;

        return healthData;
    }
}
