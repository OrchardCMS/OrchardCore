using Microsoft.AspNetCore.Authorization;

namespace OrchardCore.SignalR;

/// <summary>
/// Requires an authenticated SignalR caller and allows the hub to pre-authenticate additional schemes before authorization runs.
/// </summary>
/// <remarks>
/// This attribute behaves like <see cref="AuthorizeAttribute"/> for hub authorization while also allowing
/// token-based SignalR clients to authenticate with one or more non-default schemes, such as Orchard Core's
/// <c>Api</c> bearer scheme, without replacing the tenant's normal default challenge behavior.
/// </remarks>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public sealed class AuthorizeSignalRAttribute : AuthorizeAttribute
{
    /// <summary>
    /// Gets or sets the comma-separated list of additional authentication schemes that SignalR should try before authorization runs.
    /// </summary>
    /// <remarks>
    /// This property is used by the SignalR middleware to pre-authenticate hub requests and intentionally does not
    /// populate <see cref="AuthorizeAttribute.AuthenticationSchemes"/>, which would otherwise replace ASP.NET Core's
    /// default challenge behavior.
    /// </remarks>
    public new string AuthenticationSchemes { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the tenant's default authenticate scheme should continue to be accepted.
    /// </summary>
    /// <remarks>
    /// This is enabled by default so signed-in browser users keep working without any extra configuration.
    /// Set it to <see langword="false"/> to require one of <see cref="AuthenticationSchemes"/> instead.
    /// </remarks>
    public bool IncludeDefaultAuthenticateScheme { get; set; } = true;
}
