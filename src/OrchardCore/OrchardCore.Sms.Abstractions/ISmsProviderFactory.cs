using System.Threading.Tasks;

namespace OrchardCore.Sms;

public interface ISmsProviderFactory
{
    Task<ISmsProvider> CreateAsync(string name = null);
}
