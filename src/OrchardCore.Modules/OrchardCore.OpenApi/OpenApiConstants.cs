namespace OrchardCore.OpenApi;

internal static class OpenApiConstants
{
    /// <summary>
    /// Name of the SSRF-guarded <see cref="System.Net.Http.IHttpClientFactory"/> client used for
    /// every outbound OAuth discovery/token request the module makes on behalf of a configured or
    /// caller-supplied URL.
    /// </summary>
    public const string OAuthValidationHttpClientName = "OrchardCore.OpenApi.OAuthValidation";
}
