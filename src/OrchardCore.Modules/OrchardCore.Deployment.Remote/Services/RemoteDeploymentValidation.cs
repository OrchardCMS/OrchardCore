using OrchardCore.Localization;

namespace OrchardCore.Deployment.Remote.Services;

/// <summary>Shared validation for the admin and remote deployment configuration paths.</summary>
/// <remarks>Its messages are declared in the context of this type, which callers localize them with.</remarks>
public static class RemoteDeploymentValidation
{
    /// <summary>Validates the required inbound client credentials.</summary>
    public static Dictionary<string, string[]> Client(string clientName, string apiKey)
    {
        var errors = new Dictionary<string, string[]>();
        if (string.IsNullOrWhiteSpace(clientName) || clientName.Length > 256)
        {
            errors["ClientName"] = [LocalizationSource.Create("Provide a client name of at most 256 characters.", typeof(RemoteDeploymentValidation)).Value];
        }
        if (string.IsNullOrWhiteSpace(apiKey) || apiKey.Length > 4096)
        {
            errors["ApiKey"] = [LocalizationSource.Create("Provide an API key of at most 4096 characters.", typeof(RemoteDeploymentValidation)).Value];
        }
        return errors;
    }

    /// <summary>Validates an outbound destination without making a network request.</summary>
    public static Dictionary<string, string[]> Instance(string name, string url, string clientName, string apiKey)
    {
        var errors = Client(clientName, apiKey);
        if (string.IsNullOrWhiteSpace(name) || name.Length > 256)
        {
            errors["Name"] = [LocalizationSource.Create("Provide a name of at most 256 characters.", typeof(RemoteDeploymentValidation)).Value];
        }
        if (!IsSafeUrl(url))
        {
            errors["Url"] = [LocalizationSource.Create("Provide an HTTPS URL without user information or a fragment; HTTP is allowed only for loopback development.", typeof(RemoteDeploymentValidation)).Value];
        }
        return errors;
    }

    /// <summary>Checks the transport requirements before sending deployment credentials.</summary>
    public static bool IsSafeUrl(string url) => url?.Length <= 2048
        && Uri.TryCreate(url, UriKind.Absolute, out var uri)
        && (uri.Scheme == Uri.UriSchemeHttps || uri.Scheme == Uri.UriSchemeHttp && uri.IsLoopback)
        && string.IsNullOrEmpty(uri.UserInfo) && string.IsNullOrEmpty(uri.Fragment);
}
