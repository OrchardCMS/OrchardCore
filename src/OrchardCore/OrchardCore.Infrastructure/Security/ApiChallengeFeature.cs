namespace OrchardCore.Security;

/// <summary>
/// Marks the current response as an API response produced by the "Api" authentication scheme,
/// i.e. a challenge (401) or a forbid (403) issued by <see cref="ApiAuthenticationHandler"/> or
/// by the scheme it forwards to. Such responses opt out of the HTML error pages the Diagnostics
/// feature can substitute and get an RFC 9457 Problem Details body instead, attached by
/// <see cref="ApiProblemDetailsMiddleware"/>.
/// </summary>
public sealed class ApiChallengeFeature
{
    public static readonly ApiChallengeFeature Instance = new();
}
