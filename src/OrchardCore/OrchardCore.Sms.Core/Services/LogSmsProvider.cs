using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace OrchardCore.Sms.Services;

public class LogSmsProvider : ISmsProvider
{
    public const string TechnicalName = "Log";

    protected readonly IStringLocalizer S;
    private readonly ILogger _logger;

    public LocalizedString Name => S["Log - writes messages to the logs"];

    public LogSmsProvider(
        IStringLocalizer<LogSmsProvider> stringLocalizer,
        ILogger<LogSmsProvider> logger)
    {
        S = stringLocalizer;
        _logger = logger;
    }

    public Task<SmsResult> SendAsync(SmsMessage message)
    {
        _logger.LogWarning("A message with the body '{body}' was set to '{phoneNumber}'.", message.Body, message.To);

        return Task.FromResult(SmsResult.Success);
    }
}
