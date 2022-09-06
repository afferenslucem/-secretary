﻿using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using secretary.logging;
using secretary.storage.models;
using secretary.telegram.commands.caches;
using secretary.telegram.exceptions;

namespace secretary.telegram.commands.registermail;

public class EnterEmailCommand : Command
{
    private readonly ILogger<EnterEmailCommand> _logger = LogPoint.GetLogger<EnterEmailCommand>();
    public override Task Execute()
    {
        _logger.LogInformation($"{ChatId}: started register mail");
        
        return Context.TelegramClient.SendMessage(ChatId, "Введите вашу почту, с которой вы отправляете заявления.\r\n" +
                                                          @"Например: <i>a.pushkin@infinnity.ru</i>");
    }

    public override async Task<int> OnMessage()
    {
        var cache = new RegisterMailCache(Message);

        await Context.CacheService.SaveEntity(ChatId, cache);
        
        return ExecuteDirection.RunNext;
    }

    public override async Task ValidateMessage()
    {
        var emailRegex = new Regex(@"^[\w_\-\.]+@([\w\-_]+\.)+[\w-]{2,4}");

        if (!emailRegex.IsMatch(Message))
        {
            await this.Context.TelegramClient.SendMessage(ChatId, "Некорректный формат почты. Введите почту еще раз");
            throw new IncorrectFormatException();
        }
    }
}