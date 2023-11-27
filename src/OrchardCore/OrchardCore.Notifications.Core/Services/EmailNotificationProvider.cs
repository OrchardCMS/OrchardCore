using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.Email;
using OrchardCore.Users.Models;

namespace OrchardCore.Notifications.Services;

public class EmailNotificationProvider : INotificationMethodProvider
{
    private readonly ISmtpService _smtpService;
    protected readonly IStringLocalizer S;

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

        var result = await _smtpService.SendAsync(user.Email, message.Summary, body, isHtmlBody: isHtmlBody);

        return result.Succeeded;
    }
}
