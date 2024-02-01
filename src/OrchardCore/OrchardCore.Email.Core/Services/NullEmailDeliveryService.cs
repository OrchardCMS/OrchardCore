using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace OrchardCore.Email.Services;

public class NullEmailDeliveryService : IEmailDeliveryService
{
    private readonly ILogger _logger;
    private readonly IStringLocalizer S;

    public NullEmailDeliveryService(ILogger<NullEmailDeliveryService> logger, IStringLocalizer<NullEmailDeliveryService> stringLocalizer)
    {
        _logger = logger;
        S = stringLocalizer;
    }

    public async Task<EmailResult> DeliverAsync(MailMessage message)
    {
        _logger.LogWarning("No email delivery service is configured. Please enable an actual implementation so email can be sent.");

        return await Task.FromResult(EmailResult.Failed(S["Please enable an actual implementation so email can be sent."]));
    }
}
