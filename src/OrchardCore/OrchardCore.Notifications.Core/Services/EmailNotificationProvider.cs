using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.Email;
using OrchardCore.Notifications.Models;
using OrchardCore.Users;
using OrchardCore.Users.Models;

namespace OrchardCore.Notifications.Services;

public class EmailNotificationProvider : INotificationMethodProvider
{
    private readonly ISmtpService _smtpService;
    private readonly IStringLocalizer S;

    public EmailNotificationProvider(ISmtpService smtpService, IStringLocalizer<EmailNotificationProvider> stringLocalizer)
    {
        _smtpService = smtpService;
        S = stringLocalizer;
    }

    public string Method => "Email";

    public LocalizedString Name => S["Email Notifications"];

    public async Task<bool> TrySendAsync(IUser user, INotificationMessage message)
    {
        if (user is not User su)
        {
            return false;
        }

        var emailMessage = new MailMessage()
        {
            To = su.Email,
            Subject = message.Subject,
            BodyText = message.Body,
            IsBodyHtml = false,
        };

        if (message is HtmlNotificationMessage emailNotificationMessage && emailNotificationMessage.BodyContainsHtml)
        {
            emailMessage.Body = emailMessage.Body;
            emailMessage.BodyText = null;
            emailMessage.IsBodyText = true;
        }

        var result = await _smtpService.SendAsync(emailMessage);

        return result.Succeeded;
    }
}
