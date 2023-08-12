using System.Threading.Tasks;

namespace OrchardCore.Sms;

public interface ISmsProvider
{
    Task<SmsResult> SendAsync(SmsMessage message);
}
