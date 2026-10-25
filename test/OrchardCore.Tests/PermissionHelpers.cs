using OrchardCore.Localization;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Tests;

public class PermissionHelpers
{
    /// <summary>
    /// Create a permission with required <see cref="Permission.Name"/> and <see cref="Permission.Description"/>.
    /// </summary>
    public static Permission Create(string name, string description, params IEnumerable<Permission> impliedBy)
    {
        ArgumentException.ThrowIfNullOrEmpty(description);
        return new Permission(name, LocalizationSource.Create(description), impliedBy);
    }
}
