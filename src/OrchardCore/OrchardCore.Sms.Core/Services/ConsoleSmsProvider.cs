using System.Diagnostics;
using System.Threading.Tasks;

namespace OrchardCore.Sms.Services;

public class ConsoleSmsProvider : ISmsProvider
{
    public Task<SmsResult> SendAsync(SmsMessage message)
    {
        Debug.WriteLine($"A message with the body {message.Body} was set to {message.To}.");

        return Task.FromResult(SmsResult.Success);
    }
}
