namespace OrchardCore.OpenId.Abstractions.Handlers;

/// <summary>
/// Provides a hook for adding custom claims to the userinfo endpoint response.
/// Implementations can inspect <see cref="UserInfoClaimsContext.Principal"/> and add entries to
/// <see cref="UserInfoClaimsContext.Claims"/>.
/// Standard scope-based claims (profile, email, phone, roles) have already been written
/// before this method is called. Callers are responsible for ensuring that added claims
/// comply with the OpenID Connect specification.
/// </summary>
public interface IUserInfoClaimsProvider
{
    /// <summary>
    /// Generates additional claims and adds them to the userinfo claims dictionary.
    /// </summary>
    /// <param name="context">
    /// The context carrying the authenticated principal and the claims dictionary being built.
    /// </param>
    Task GenerateAsync(UserInfoClaimsContext context);
}
