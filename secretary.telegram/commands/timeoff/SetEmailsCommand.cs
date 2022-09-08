﻿using secretary.storage.models;
using secretary.telegram.commands.caches;
using secretary.telegram.exceptions;
using secretary.telegram.utils;

namespace secretary.telegram.commands.timeoff;

public class SetEmailsCommand : Command
{
    public override async Task Execute()
    {
        if (Message.ToLower() != "да" && !Context.BackwardRedirect)
        {
            await this.CancelCommand();
            this.ForceComplete();
        }

        var document = await DocumentStorage.GetOrCreateDocument(TimeOffCommand.Key);
        var emails = await EmailStorage.GetForDocument(document.Id);

        if (emails.Count() > 0)
        {
            await this.SendRepeat(emails);
        }
        else
        {
            await this.SendAskEmails();
        }
    }

    public Task CancelCommand()
    {
        return TelegramClient.SendMessage("Дальнейшее выполнение команды прервано");
    }

    public Task SendAskEmails()
    {
        return TelegramClient.SendMessage( 
            "Отправьте список адресов для рассылки в формате:\n" +
            "<code>" +
            "a.pushkin@infinnity.ru (Александр Пушкин)\n" +
            "s.esenin@infinnity.ru (Сергей Есенин)\n" +
            "v.mayakovskii@infinnity.ru\n" +
            "</code>\n\n" +
            "Если вы укажете адрес без имени в скобках, то в имени отправителя будет продублированпочтовый адрес");
    }

    public Task SendRepeat(IEnumerable<Email> emails)
    {
        var emailsPrints = emails
            .Select(item => item.DisplayName != null ? $"{item.Address} ({item.DisplayName})" : item.Address);

        var emailTable = string.Join("\n", emailsPrints);

        var message = "В прошлый раз вы сделали рассылку на эти адреса:\n" +
                      "<code>\n" +
                      $"{emailTable}" +
                      "</code>\n" +
                      "\n" +
                      "Повторить?";
        
        return TelegramClient.SendMessageWithKeyBoard(message, new [] { "Повторить" });
    }

    public override async Task<int> OnMessage()
    {
        if (Message == "Повторить")
        {
            return 2;
        }
        else
        {
            return await ParseAndSaveEmails();
        }
    }

    private async Task<int> ParseAndSaveEmails()
    {
        try
        {
            var cache = await CacheService.GetEntity<TimeOffCache>();
            if (cache == null) throw new InternalException();
            
            cache.Emails = new EmailParser().ParseMany(Message);
            await CacheService.SaveEntity(cache);

            return ExecuteDirection.RunNext;
        }
        catch (IncorrectEmailException e)
        {
            await TelegramClient.SendMessage($"Почтовый адрес <code>{e.IncorrectEmail}</code> имеет некорректный формат.\n" +
                "Поправьте его и отправте список адресов еще раз.");

            return ExecuteDirection.Retry;
        }
    }
}