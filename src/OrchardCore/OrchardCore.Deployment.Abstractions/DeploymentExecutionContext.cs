using System.Security.Claims;

namespace OrchardCore.Deployment;

/// <summary>
/// Carries the initiating principal within one deployment execution scope, including queued exports.
/// </summary>
public sealed class DeploymentExecutionContext
{
    /// <summary>Gets or sets the principal whose permissions deployment sources must enforce.</summary>
    public ClaimsPrincipal User { get; set; }
}
