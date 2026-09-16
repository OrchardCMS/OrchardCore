using System.Text.Json.Nodes;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Settings;

/// <summary>
/// Explicitly exposes one module-owned settings section. Providers must allowlist reads and writes,
/// preserve omitted fields, validate null/reset semantics and redact secrets before returning values.
/// Registration follows the owning feature; it does not expose arbitrary site properties.
/// </summary>
public interface ISiteSettingsSectionProvider
{
    /// <summary>The stable section descriptor. Names must be unique among enabled providers.</summary>
    SiteSettingsSectionDescriptor Descriptor { get; }
    /// <summary>The permission needed to discover and read this section.</summary>
    Permission ReadPermission { get; }
    /// <summary>The permission needed to update this section.</summary>
    Permission UpdatePermission { get; }
    /// <summary>Returns an explicit update schema, including allowed nulls and secret write-only fields.</summary>
    JsonObject GetSchema();
    /// <summary>Returns allowlisted, redacted values with configuration ownership and read-only status.</summary>
    Task<SiteSettingsSectionResponse> GetAsync();
    /// <summary>
    /// Applies an allowlisted partial update, preserving omitted members and replacing supplied arrays.
    /// Providers own validation, configuration ownership, persistence and required cache/shell changes.
    /// Returned values must be as safe as readback; secrets must never be echoed.
    /// </summary>
    Task<SiteSettingsSectionUpdateResult> UpdateAsync(JsonObject values);
}

/// <summary>Describes a settings section contributed by an enabled feature.</summary>
public sealed class SiteSettingsSectionDescriptor
{
    /// <summary>The stable section identifier used in API paths.</summary>
    public string Name { get; init; }
    /// <summary>The display name.</summary>
    public string DisplayName { get; init; }
    /// <summary>The owning feature identifier.</summary>
    public string FeatureId { get; init; }
    /// <summary>The section's scope and update semantics.</summary>
    public string Description { get; init; }
    /// <summary>Whether updates must arrive over HTTPS, as determined by the server request scheme.</summary>
    public bool RequiresHttps { get; init; }
    /// <summary>Whether changing this section requires reloading the tenant pipeline.</summary>
    public bool RequiresReload { get; init; }
}

/// <summary>Allowlisted section values and their configuration ownership.</summary>
public sealed class SiteSettingsSectionResponse
{
    /// <summary>The canonical section name.</summary>
    public string Name { get; init; }
    /// <summary>The effective source: tenant, configuration or mixed, as explicitly determined by the provider.</summary>
    public string Source { get; init; }
    /// <summary>Whether the whole section is owned elsewhere and cannot be updated through this API.</summary>
    public bool IsReadOnly { get; init; }
    /// <summary>The reason the section is read-only, when applicable.</summary>
    public string ReadOnlyReason { get; init; }
    /// <summary>Allowlisted values. Secret values must be omitted; a provider must never serialize the whole site object.</summary>
    public JsonObject Values { get; init; } = [];
    /// <summary>Property names omitted from values because their contents are secret.</summary>
    public string[] RedactedProperties { get; init; } = [];
    /// <summary>Individual properties controlled by configuration rather than tenant updates.</summary>
    public string[] ReadOnlyProperties { get; init; } = [];
}

/// <summary>Result of a validated section update.</summary>
public sealed class SiteSettingsSectionUpdateResult
{
    /// <summary>The safe readback after a successful update or equivalent retry.</summary>
    public SiteSettingsSectionResponse Section { get; init; }
    /// <summary>Whether stored values actually changed.</summary>
    public bool Changed { get; init; }
    /// <summary>Whether this update requested a tenant reload.</summary>
    public bool ReloadRequested { get; init; }
    /// <summary>Validation errors keyed by update property; error messages must not echo secrets.</summary>
    public Dictionary<string, string[]> Errors { get; init; } = [];
}
