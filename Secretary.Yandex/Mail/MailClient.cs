﻿using MailKit;
using MailKit.Net.Imap;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using MimeKit.Text;
using MimeKit.Utils;
using Secretary.Logging;
using Secretary.Yandex.Exceptions;
using Serilog;

namespace Secretary.Yandex.Mail;

public class MailClient: IMailClient
{
    private ILogger _logger = LogPoint.GetLogger<MailClient>();
    
    public async Task SendMail(SecretaryMailMessage messageConfig)
    {
        _logger.Debug("Sending message");

        using var message = await SendEmail(messageConfig);

        try
        {
            await PutToSent(message, messageConfig);
        }
        catch (YandexApiException e)
        {
            if (e.Message == "Could not move message to sent")
            {
                _logger.Error(e, "Could not put message to Sent");
                await ForwardMessage(message, messageConfig);
            }
            else
            {
                _logger.Error(e, "Error during sending message");
                throw;
            }
        }
        catch (Exception e)
        {
            _logger.Error(e, "Error during sending message");
            throw;
        }
    }

    private async Task<MimeMessage> SendEmail(SecretaryMailMessage messageConfig)
    {
        using var client = new SmtpClient();
        
        await client.ConnectAsync("smtp.yandex.ru", 465, true);
        
        var oauth2 = new SaslMechanismOAuth2(messageConfig.Sender.Address, messageConfig.Token);
        await client.AuthenticateAsync(oauth2);

        var receivers = messageConfig.Receivers
            .Select(item => new MailboxAddress(item.DisplayName ?? item.Address, item.Address))
            .ToArray();

        var html = new TextPart(TextFormat.Html);
        html.Text = messageConfig.HtmlBody;

        var attachments = messageConfig.Attachments.Select(item => new MimePart(item.ContentType)
        {
            Content = new MimeContent(File.OpenRead(item.Path)),
            ContentDisposition = new ContentDisposition(ContentDisposition.Attachment),
            ContentTransferEncoding = ContentEncoding.Base64,
            FileName = item.FileName
        });
        
        var multipart = new Multipart("mixed");
        
        multipart.Add(html);

        foreach (var attachment in attachments)
        {
            multipart.Add(attachment);
        }
        
        var message = new MimeMessage(
            new [] { new MailboxAddress(messageConfig.Sender.DisplayName, messageConfig.Sender.Address) },
            receivers,
            messageConfig.Theme,
            multipart
        );
        
        await client.SendAsync(message);

        await client.DisconnectAsync(true);

        return message;
    }
    
    private async Task PutToSent(MimeMessage message, SecretaryMailMessage messageConfig)
    {
        try
        {
            using var imap = new ImapClient();

            await imap.ConnectAsync("imap.yandex.ru", 993, true);

            var oauth2 = new SaslMechanismOAuth2(messageConfig.Sender.Address, messageConfig.Token);
            await imap.AuthenticateAsync(oauth2);

            var personal = imap.GetFolder(imap.PersonalNamespaces[0]);
            var sent = await personal.GetSubfolderAsync("Sent");

            await sent.AppendAsync(message, MessageFlags.Seen);

            await imap.DisconnectAsync(true).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            throw new YandexApiException("Could not move message to sent", e);
        }
    }

    private async Task ForwardMessage(MimeMessage message, SecretaryMailMessage messageConfig)
    {
        using var client = new SmtpClient();
        await client.ConnectAsync("smtp.yandex.ru", 465, true);
        var oauth2 = new SaslMechanismOAuth2(messageConfig.Sender.Address, messageConfig.Token);
        await client.AuthenticateAsync(oauth2);

        var sender = messageConfig.Sender;
        var senderAddress = new MailboxAddress(sender.DisplayName, sender.Address);
        
        message.ResentFrom.Add (senderAddress);
        message.ResentTo.Add (senderAddress);
        message.ResentMessageId = MimeUtils.GenerateMessageId ();
        message.ResentDate = DateTimeOffset.Now;
        
        await client.SendAsync(message);

        await client.DisconnectAsync(true);
    }
}