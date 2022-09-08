﻿using Microsoft.Extensions.Logging;
using secretary.logging;
using secretary.telegram.commands.caches;
using secretary.telegram.commands.validation;
using secretary.telegram.utils;

namespace secretary.telegram.commands.timeoff;

public class EnterPeriodCommand : Command
{
    private readonly ILogger<EnterPeriodCommand> _logger = LogPoint.GetLogger<EnterPeriodCommand>();

    public override async Task Execute()
    {
        await ValidateUser();
        
        _logger.LogInformation($"{ChatId}: Started time off");

        await TelegramClient.SendMessage("Введите период отгула в формате <strong>DD.MM.YYYY[ с HH:mm до HH:mm]</strong>\r\n" +
                                         "Например: <i>26.04.2020 c 9:00 до 13:00</i>\r\n" +
                                         "Или: <i>26.04.2020</i>, если вы берете отгул на целый день\r\n" +
                                         "В таком виде это будет вставлено в документ");
    }
    
    public override async Task<int> OnMessage()
    {
        var cache = new TimeOffCache();

        var period = new DatePeriodParser().Parse(Message);

        cache.Period = period;

        await CacheService.SaveEntity(cache);
        
        return ExecuteDirection.RunNext;
    }

    private async Task ValidateUser()
    {
        var user = await UserStorage.GetUser();
        
        new UserValidationVisitor().Validate(user);
    }
}