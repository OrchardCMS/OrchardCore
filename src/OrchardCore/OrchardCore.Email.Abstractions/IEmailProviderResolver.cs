using System.Threading.Tasks;

namespace OrchardCore.Email;

public interface IEmailProviderResolver
{
    /// <summary>
    /// Gets the Email provider for the given technical name.
    /// When null or empty string is provided, it returns the default email provider.
    /// </summary>
    /// <param name="name">The technical name of the Email provider.</param>
    /// <returns>Instance IEmailProvider or null when no default service is available.</returns>
    /// <exception cref="InvalidEmailProviderException">When invalid provider is given.</exception>
    Task<IEmailProvider> GetAsync(string name = null);
}
