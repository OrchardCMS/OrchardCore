using System.Security.Claims;
using System.Text.Json.Nodes;

namespace OrchardCore.Settings;

/// <summary>
/// Provides explicitly described site settings schemas for remote discovery.
/// </summary>
public interface ISiteSettingsManagementSchemaProvider
{
    /// <summary>
    /// Returns schema sections visible to the specified authenticated user.
    /// </summary>
    /// <param name="user">The authenticated user requesting schema discovery.</param>
    /// <returns>The explicitly contributed schema sections.</returns>
    ValueTask<IEnumerable<SiteSettingsManagementSchemaSection>> GetSchemaSectionsAsync(ClaimsPrincipal user);
}

/// <summary>
/// Describes a contributed site settings schema section.
/// </summary>
public sealed class SiteSettingsManagementSchemaSection
{
    /// <summary>
    /// Gets or sets the stable section name.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the section display name.
    /// </summary>
    public string DisplayName { get; set; }

    /// <summary>
    /// Gets or sets the section description.
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Gets or sets the explicit JSON Schema for the section.
    /// </summary>
    public JsonObject Schema { get; set; }
}
