﻿using secretary.storage.models;

namespace secretary.telegram.commands;

public class MeCommand: Command
{
    public const string Key = "/me";
    
    public override async Task Execute()
    {
        var user = await Context.UserStorage.GetUser(ChatId);

        if (user == null)
        {
            await ReturnUnregisteredUser();
            return;
        }

        if (user.AccessToken == null)
        {
            await ReturnUnregisteredMail(user);
            return;
        }

        if (user.JobTitleGenitive == null)
        {
            await ReturnUnregisteredName(user);
            return;
        }

        await ReturnUserInfo(user);
    }

    private Task ReturnUnregisteredUser()
    {
        return Context.TelegramClient.SendMessage(ChatId, "Вы незарегистрированный пользователь\r\n\r\n" +
                                                          "Для корректной работы вам необходимо выполнить следующие команды:\r\n" +
                                                          "/registeruser\r\n" +
                                                          "/registermail");
    }

    private Task ReturnUnregisteredMail(User user)
    {
        var userInfo = GetUserInfo(user);
        
        return Context.TelegramClient.SendMessage(ChatId, $"{userInfo}\r\n\r\n" +
                                                          "У вас нет токена для почты. Выполните команду /registermail");
    }

    private Task ReturnUnregisteredName(User user)
    {
        var userInfo = GetUserInfo(user);
        
        return Context.TelegramClient.SendMessage(ChatId, $"{userInfo}\r\n\r\n" +
                                                          "У вас не заданы данные о пользователе. Выполните команду /registeruser");
    }

    private Task ReturnUserInfo(User user)
    {
        var userInfo = GetUserInfo(user);
        
        return Context.TelegramClient.SendMessage(ChatId, userInfo);
    }

    private string GetUserInfo(User user)
    {
        var result = $"<strong>Имя:</strong> {user.Name ?? "не задано"}\r\n" +
                   $"<strong>Имя в Р.П.:</strong> {user.NameGenitive ?? "не задано"}\r\n" +
                   $"<strong>Должность:</strong> {user.JobTitle ?? "не задана"}\r\n" +
                   $"<strong>Должность в Р.П.:</strong> {user.JobTitleGenitive ?? "не задана"}\r\n" +
                   $"<strong>Почта:</strong> {user.Email ?? "не задана"}";

        return result;
    }
}