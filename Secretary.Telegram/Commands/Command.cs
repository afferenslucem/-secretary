﻿using Newtonsoft.Json;
using Secretary.Storage;
using Secretary.Storage.Interfaces;
using Secretary.Telegram.Exceptions;
using Secretary.Telegram.Wrappers;
using Secretary.Yandex.Authentication;
using Secretary.Yandex.Mail;

namespace Secretary.Telegram.Commands;

public abstract class Command
{
    protected readonly CancellationTokenSource CancellationToken;

    [JsonIgnore]
    public CommandContext Context { get; set; } = null!;

    protected TelegramClientWrapper TelegramClient => new (Context.TelegramClient, Context.ChatId);
    protected SessionStorageWrapper SessionStorage => new (Context.SessionStorage, Context.ChatId);
    protected UserStorageWrapper UserStorage => new (Context.UserStorage, Context.ChatId);
    protected DocumentStorageWrapper DocumentStorage => new (Context.DocumentStorage, Context.ChatId);
    protected IEmailStorage EmailStorage => Context.EmailStorage;
    protected IYandexAuthenticator YandexAuthenticator => Context.YandexAuthenticator;
    protected IMailSender MailSender => Context.MailSender;
    protected CacheServiceWrapper CacheService => new (Context.CacheService, Context.ChatId);
    protected StatisticService StatisticService => new (Context.EventLogStorage);

    protected long ChatId => Context.ChatId;

    protected string Message => Context.Message;

    protected Command()
    {
        CancellationToken = new CancellationTokenSource();
    }

    public abstract Task Execute();

    public virtual Task<int> OnMessage()
    {
        return Task.FromResult(ExecuteDirection.RunNext);
    }

    public virtual Task ValidateMessage()
    {
        return Task.CompletedTask;
    }

    public virtual Task Cancel()
    {
        this.CancellationToken.Cancel();
        
        return Task.CompletedTask;
    }

    protected internal void ForceComplete()
    {
        throw new ForceCompleteCommandException(this.GetType().Name);
    }

    public virtual async Task OnForceComplete()
    {
        await SessionStorage.DeleteSession();
    }
    
    public virtual async Task OnComplete()
    {
        await SessionStorage.DeleteSession();
    }
}