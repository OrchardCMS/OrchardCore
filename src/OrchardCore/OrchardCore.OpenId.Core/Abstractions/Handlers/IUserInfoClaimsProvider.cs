namespace OrchardCore.OpenId.Abstractions.Handlers;

/// <summary>
/// Provides a hook for adding custom claims to the userinfo endpoint response.
/// Implementations can inspect <see cref="UserInfoClaimsContext.Principal"/>, including
/// its granted scopes via <c>HasScope()</c>, and add entries to
/// <see cref="UserInfoClaimsContext.Claims"/>.
/// Standard scope-based claims (profile, email, phone, roles) have already been written
/// before this method is called. Callers are responsible for ensuring that added claims
/// comply with the OpenID Connect specification.
/// </summary>
public interface IUserInfoClaimsProvider
{
    /// <summary>
    /// Populates the userinfo claims dictionary with additional entries.
    /// </summary>
    /// <param name="context">
    /// The context carrying the authenticated principal and the claims dictionary being built.
    /// </param>
    Task PopulateAsync(UserInfoClaimsContext context);
}
