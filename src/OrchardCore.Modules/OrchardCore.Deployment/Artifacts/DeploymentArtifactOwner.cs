using System.Security.Claims;
using System.Text.Json;

namespace OrchardCore.Deployment.Artifacts;

internal static class DeploymentArtifactOwner
{
    public static string Get(ClaimsPrincipal principal)
    {
        if (principal.Identity?.IsAuthenticated != true) { return null; }
        // Orchard OpenID's access-token entity discriminator distinguishes equal user/client subjects.
        var kind = principal.FindFirst("oc:entyp")?.Value;
        var subject = principal.FindFirst("sub")?.Value ?? principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (kind is not ("user" or "application") || string.IsNullOrWhiteSpace(subject)) { return null; }
        return JsonSerializer.Serialize(new[] { principal.FindFirst("iss")?.Value ?? string.Empty, kind, subject });
    }
}
