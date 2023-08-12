using System;
using System.Threading.Tasks;
using OrchardCore.Sms.Abstractions;

namespace OrchardCore.Sms.Services;

public class TwilioSmsService : ISmsService
{
    public Task<bool> SendAsync(SmsMessage message)
    {
        Console.WriteLine($"Twilio SMS Service: A message with the body {message.Body} was set to {message.To}.");

        return Task.FromResult(true);
    }
}
