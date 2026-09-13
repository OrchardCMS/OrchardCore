using OrchardCore.RateLimits.Models;

namespace OrchardCore.RateLimits.Core;

/// <summary>Identifies a domain validation failure in a rate-limit policy target.</summary>
public enum RateLimitPolicyTargetError
{
    /// <summary>The target is valid.</summary>
    None,
    /// <summary>The scope is not supported.</summary>
    InvalidScope,
    /// <summary>An endpoint policy has no path.</summary>
    MissingPath,
    /// <summary>An endpoint path does not begin with a slash.</summary>
    RelativePath,
    /// <summary>A group policy has no group name.</summary>
    MissingGroup,
}

/// <summary>Validates target rules shared by administration, recipes and remote management.</summary>
public static class RateLimitPolicyValidation
{
    /// <summary>Validates a scope and its required target without changing caller-owned values.</summary>
    public static RateLimitPolicyTargetError ValidateTarget(RateLimitPolicyScope scope, string path, string groupName)
    {
        if (scope is not RateLimitPolicyScope.Global and not RateLimitPolicyScope.Endpoint and not RateLimitPolicyScope.Group)
        {
            return RateLimitPolicyTargetError.InvalidScope;
        }

        if (scope == RateLimitPolicyScope.Endpoint)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return RateLimitPolicyTargetError.MissingPath;
            }

            if (!path.StartsWith('/'))
            {
                return RateLimitPolicyTargetError.RelativePath;
            }
        }

        return scope == RateLimitPolicyScope.Group && string.IsNullOrWhiteSpace(groupName)
            ? RateLimitPolicyTargetError.MissingGroup : RateLimitPolicyTargetError.None;
    }
}
