using System;
using System.Threading.Tasks;

namespace OrchardCore.Sms.Services;

public class TwilioSmsProvider : ISmsProvider
{
    public Task<SmsResult> SendAsync(SmsMessage message)
    {
        Console.WriteLine($"Twilio SMS Service: A message with the body {message.Body} was set to {message.To}.");

        return Task.FromResult(SmsResult.Success);
    }
}
