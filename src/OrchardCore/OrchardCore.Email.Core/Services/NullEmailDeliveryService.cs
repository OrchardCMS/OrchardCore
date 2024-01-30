using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace OrchardCore.Email.Services;

public class NullEmailDeliveryService : IEmailDeliveryService
{
    private readonly ILogger _logger;

    public NullEmailDeliveryService(ILogger<NullEmailDeliveryService> logger)
    {
        _logger = logger;
    }

    public async Task<EmailResult> DeliverAsync(MailMessage message)
    {
        _logger.LogWarning("No email delivery service is configured. Please enable an actual implementation so email can be sent.");

        return await Task.FromResult(EmailResult.Success);
    }
}
