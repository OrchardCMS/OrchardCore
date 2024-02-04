using System.Threading.Tasks;

namespace OrchardCore.Email;

public interface IEmailProviderResolver
{
    /// <summary>
    /// Gets the Email provider for the technical given name.
    /// When null or empty string is provided, it returns the default SMS provider.
    /// </summary>
    /// <param name="name">The key of the SMS provider.</param>
    /// <returns>Instance ISmsProvider or null when no service found.</returns>
    Task<IEmailProvider> GetAsync(string name = null);
}
