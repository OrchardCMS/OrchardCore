using System.Threading.Tasks;

namespace OrchardCore.Sms;

public interface ISmsProviderResolver
{
    /// <summary>
    /// Gets the SMS provider for the given name.
    /// When null is null or empty, it gets the default SMS provider.
    /// </summary>
    /// <param name="name">The key of the SMS provider</param>
    /// <returns>Instance ISmsProvider or null when no service found.</returns>
    Task<ISmsProvider> GetAsync(string name = null);
}
