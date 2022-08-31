﻿using secretary.telegram.commands.executors;

namespace secretary.telegram.commands;

public class NullCommand: Command
{
    public const string Key = "*";
    
    public override async Task Execute()
    {
        var session = await Context.GetSession();

        if (session == null || session.LastCommand == null)
        {
            await Context.TelegramClient.SendMessage(ChatId, "Извините, я не понял\r\nОтправьте команду");
            
            return;
        };

        await new CommandExecutor(session.LastCommand, Context).OnMessage();
    }
}