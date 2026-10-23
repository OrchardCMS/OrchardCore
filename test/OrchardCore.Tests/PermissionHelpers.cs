using OrchardCore.Localization;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Tests;

public class PermissionHelpers
{
    /// <summary>
    /// Create a permission with <see cref="Permission.Name"/> and <see cref="Permission.Description"/>.
    /// </summary>
    /// <param name="name">The technical <see cref="Permission.Name"/>.</param>
    /// <param name="description">
    /// The <see cref="Permission.Description"/>. If this is <see langword="null"/> or empty, then <paramref
    /// name="name"/> is used instead.
    /// </param>
    public static Permission Create(string name, string description = null, params IEnumerable<Permission> impliedBy) =>
        new(
            name,
            LocalizationSource.Create(string.IsNullOrEmpty(description) ? name : description),
            impliedBy);
}
