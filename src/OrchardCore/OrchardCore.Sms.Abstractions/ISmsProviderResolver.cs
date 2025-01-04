namespace OrchardCore.Sms;

public interface ISmsProviderResolver
{
    /// <summary>
    /// Gets the SMS provider for the technical given name.
    /// When null or empty string is provided, it returns the default SMS provider.
    /// </summary>
    /// <param name="name">The key of the SMS provider.</param>
    /// <returns>Instance ISmsProvider or null when no service found.</returns>
    Task<ISmsProvider> GetAsync(string name = null);
}
