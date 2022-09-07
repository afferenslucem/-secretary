﻿using Microsoft.Extensions.Logging;
using secretary.logging;

namespace secretary.telegram.commands;

public class CancelCommand: Command
{
    private readonly ILogger<CancelCommand> _logger = LogPoint.GetLogger<CancelCommand>();
    
    public const string Key = "/cancel";
    
    public override async Task Execute()
    {
        var session = await SessionStorage.GetSession();
        
        session?.LastCommand?.Cancel();
        
        await SessionStorage.DeleteSession();
        await TelegramClient.SendMessage("Дальнейшее выполнение команды прервано");

        var commandTypeName = session?.LastCommand?.GetType()?.Name ?? "null";
        
        _logger.LogInformation($"{ChatId}: Cancelled command {commandTypeName}");
    }

    public override Task OnComplete()
    {
        return Task.CompletedTask;
    }
}