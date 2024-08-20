using Microsoft.Extensions.Localization;
using OrchardCore.Email;
using OrchardCore.Users.Models;

namespace OrchardCore.Notifications.Services;

public class EmailNotificationProvider : INotificationMethodProvider
{
    private readonly IEmailService _emailService;
    protected readonly IStringLocalizer S;

    public EmailNotificationProvider(
        IEmailService emailService,
        IStringLocalizer<EmailNotificationProvider> stringLocalizer)
    {
        _emailService = emailService;
        S = stringLocalizer;
    }

    public string Method { get; } = "Email";

    public LocalizedString Name => S["Email Notifications"];

    public async Task<bool> TrySendAsync(object notify, INotificationMessage message)
    {
        var user = notify as User;

        if (string.IsNullOrEmpty(user?.Email))
        {
            return false;
        }

        string body;
        bool isHtmlBody;

        if (message.IsHtmlPreferred && !string.IsNullOrWhiteSpace(message.HtmlBody))
        {
            body = message.HtmlBody;
            isHtmlBody = true;
        }
        else
        {
            body = message.TextBody;
            isHtmlBody = false;
        }

        var result = await _emailService.SendAsync(user.Email, message.Subject, body, isHtmlBody);

        return result.Succeeded;
    }
}
