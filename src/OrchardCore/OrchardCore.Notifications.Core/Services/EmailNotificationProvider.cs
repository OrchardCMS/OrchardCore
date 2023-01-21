using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.Email;
using OrchardCore.Users.Models;

namespace OrchardCore.Notifications.Services;

public class EmailNotificationProvider : INotificationMethodProvider
{
    private readonly ISmtpService _smtpService;
    private readonly IStringLocalizer S;

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

        if (user == null)
        {
            return false;
        }

        var emailMessage = new MailMessage()
        {
            To = user.Email,
            Subject = message.Summary,
            BodyText = message.Summary,
            IsBodyHtml = false,
            IsBodyText = true,
        };

        if (message is INotificationBodyMessage emailNotificationMessage)
        {
            if (emailNotificationMessage.IsHtmlBody)
            {
                emailMessage.Body = emailNotificationMessage.Body;
                emailMessage.BodyText = null;
                emailMessage.IsBodyHtml = true;
                emailMessage.IsBodyText = false;
            }
            else
            {
                emailMessage.BodyText = emailNotificationMessage.Body;
            }
        }

        var result = await _smtpService.SendAsync(emailMessage);

        return result.Succeeded;
    }
}
