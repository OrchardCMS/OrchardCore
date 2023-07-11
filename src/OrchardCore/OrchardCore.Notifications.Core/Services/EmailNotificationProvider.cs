using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.Email;
using OrchardCore.Users.Models;

namespace OrchardCore.Notifications.Services;

public class EmailNotificationProvider : INotificationMethodProvider
{
    private readonly ISmtpService _smtpService;
#pragma warning disable IDE1006 // Naming Styles
    private readonly IStringLocalizer S;
#pragma warning restore IDE1006 // Naming Styles

    public EmailNotificationProvider(
        ISmtpService smtpService,
        IStringLocalizer<EmailNotificationProvider> stringLocalizer)
    {
        _smtpService = smtpService;
        S = stringLocalizer;
    }

    public string Method => "Email";

    public LocalizedString Name => S["Email Notifications"];

    public async Task<bool> TrySendAsync(object notify, INotificationMessage message)
    {
        var user = notify as User;

        if (String.IsNullOrEmpty(user?.Email))
        {
            return false;
        }

        var mailMessage = new MailMessage()
        {
            To = user.Email,
            Subject = message.Summary,
        };

        if (message.IsHtmlPreferred && !String.IsNullOrWhiteSpace(message.HtmlBody))
        {
            mailMessage.Body = message.HtmlBody;
            mailMessage.IsHtmlBody = true;
        }
        else
        {
            mailMessage.Body = message.TextBody;
            mailMessage.IsHtmlBody = false;
        }

        var result = await _smtpService.SendAsync(mailMessage);

        return result.Succeeded;
    }
}
