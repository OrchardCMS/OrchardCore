using System.Threading.Tasks;

namespace OrchardCore.Sms.Abstractions;

public interface ISmsService
{
    Task<bool> SendAsync(SmsMessage message);
}
