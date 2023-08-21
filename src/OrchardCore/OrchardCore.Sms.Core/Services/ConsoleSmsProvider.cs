using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;

namespace OrchardCore.Sms.Services;

public class ConsoleSmsProvider : ISmsProvider
{
    public const string TechnicalName = "Console";

    protected readonly IStringLocalizer S;

    public LocalizedString Name => S["Console"];

    public ConsoleSmsProvider(IStringLocalizer<ConsoleSmsProvider> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public Task<SmsResult> SendAsync(SmsMessage message)
    {
        Debug.WriteLine($"A message with the body {message.Body} was set to {message.To}.");

        return Task.FromResult(SmsResult.Success);
    }
}
