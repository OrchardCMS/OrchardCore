using System.Threading.Tasks;

namespace OrchardCore.Email;

public interface IEmailProviderResolver
{
    /// <summary>
    /// Gets the Email provider for the given technical name.
    /// When null or empty string is provided, it returns the default email provider.
    /// </summary>
    /// <param name="providerName">The technical name of the Email provider.</param>
    /// <returns>The matching <see cref="IEmailProvider"/> instance or <see langword="null"/> if no default service is available.</returns>
    /// <exception cref="InvalidEmailProviderException">When invalid provider is given.</exception>
    Task<IEmailProvider> GetAsync(string providerName = null);
}
