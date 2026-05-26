using Microsoft.Extensions.Localization;
using OrchardCore.Email;
using OrchardCore.Infrastructure;
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

    /// <summary>
    /// Attempts to send the specified notification message to the recipient through email.
    /// </summary>
    /// <param name="notify">The recipient or notifiable object.</param>
    /// <param name="message">The notification message to send.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A <see cref="Result"/> describing whether the email notification was sent successfully.</returns>
    public async Task<Result> TrySendAsync(object notify, INotificationMessage message, CancellationToken cancellationToken = default)
    {
        var user = notify as User;

        if (string.IsNullOrEmpty(user?.Email))
        {
            return Result.Failed(S["No email address provided."]);
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

        return await _emailService.SendAsync(user.Email, message.Subject, body, isHtmlBody, cancellationToken);
    }
}
