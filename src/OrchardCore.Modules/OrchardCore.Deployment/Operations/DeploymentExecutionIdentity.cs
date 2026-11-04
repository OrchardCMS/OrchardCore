using System.Security.Claims;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Deployment.Operations;

// Persist only identity and authorization claims, never bearer tokens or authentication properties.
// The first accepted request fixes the permission snapshot; retries cannot replace its identity.
internal sealed class DeploymentExecutionIdentity
{
    public DeploymentExecutionClaim[] Claims { get; init; } = [];

    public static DeploymentExecutionIdentity Capture(ClaimsPrincipal principal)
    {
        if (principal.Identity?.IsAuthenticated != true)
        {
            return null;
        }
        var allowed = new HashSet<string>(StringComparer.Ordinal)
        {
            ClaimTypes.NameIdentifier, ClaimTypes.Name, ClaimTypes.Role,
            "sub", "iss", "role", "oc:entyp", Permission.ClaimType,
        };
        var claims = principal.Claims.Where(claim => allowed.Contains(claim.Type))
            .Select(claim => new DeploymentExecutionClaim { Type = claim.Type, Value = claim.Value }).ToList();
        foreach (var identity in principal.Identities)
        {
            // OAuth identities can use "role" instead of ClaimTypes.Role. Preserve IsInRole semantics.
            claims.AddRange(identity.FindAll(identity.RoleClaimType).Select(claim =>
                new DeploymentExecutionClaim { Type = ClaimTypes.Role, Value = claim.Value }));
        }
        if (!string.IsNullOrEmpty(principal.Identity.Name))
        {
            claims.Add(new DeploymentExecutionClaim { Type = ClaimTypes.Name, Value = principal.Identity.Name });
        }
        return new() { Claims = claims.ToArray() };
    }

    public ClaimsPrincipal Restore() => new(new ClaimsIdentity(
        Claims.Select(claim => new Claim(claim.Type, claim.Value)), "DeploymentExecution"));
}

internal sealed class DeploymentExecutionClaim
{
    public string Type { get; init; }
    public string Value { get; init; }
}
