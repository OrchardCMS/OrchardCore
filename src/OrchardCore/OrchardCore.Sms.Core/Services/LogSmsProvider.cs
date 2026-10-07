using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using OrchardCore.Infrastructure;

namespace OrchardCore.Sms.Services;

public class LogSmsProvider : ISmsProvider
{
    public const string TechnicalName = "Log";

    private readonly ILogger _logger;

    protected readonly IStringLocalizer S;

    public LocalizedString Name => S["Log - writes messages to the logs"];

    public LogSmsProvider(
        IStringLocalizer<LogSmsProvider> stringLocalizer,
        ILogger<LogSmsProvider> logger)
    {
        S = stringLocalizer;
        _logger = logger;
    }

    /// <summary>
    /// Sends the specified SMS message by writing it to the application logs.
    /// </summary>
    /// <param name="message">The SMS message to log.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A successful <see cref="Result"/> after the message is written to the logs.</returns>
    public Task<Result> SendAsync(SmsMessage message, CancellationToken cancellationToken = default)
    {
        _logger.LogWarning("A message with the body '{Body}' was set to '{PhoneNumber}'.", message.Body, message.To);

        return Task.FromResult(Result.Success());
    }
}
