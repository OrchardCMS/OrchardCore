using System.Text.Json.Nodes;
using OrchardCore.Entities;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Security.Permissions;
using OrchardCore.Settings;

namespace OrchardCore.Media.Services;

internal sealed class MediaUploadSettingsSectionProvider : ISiteSettingsSectionProvider
{
    private readonly ISiteService _siteService;
    private readonly IShellReleaseManager _release;
    private readonly MediaOptions _hostOptions;

    public MediaUploadSettingsSectionProvider(ISiteService siteService, IShellReleaseManager release, IShellConfiguration configuration)
    {
        _siteService = siteService;
        _release = release;
        _hostOptions = new MediaOptions();
        new MediaOptionsConfiguration(configuration).Configure(_hostOptions);
    }

    public SiteSettingsSectionDescriptor Descriptor { get; } = new()
    {
        Name = "media-upload-policy", DisplayName = "Media upload policy", FeatureId = "OrchardCore.Media", RequiresReload = true,
        Description = "Optional tenant restrictions on the host media size and extension limits. Omission preserves overrides; null inherits the host policy.",
    };
    public Permission ReadPermission => MediaPermissions.ManageMediaApiSettings;
    public Permission UpdatePermission => MediaPermissions.ManageMediaApiSettings;

    public JsonObject GetSchema() => new()
    {
        ["type"] = "object", ["additionalProperties"] = false,
        ["properties"] = new JsonObject
        {
            ["maxFileSize"] = new JsonObject { ["type"] = new JsonArray("integer", "null"), ["minimum"] = 1, ["maximum"] = _hostOptions.MaxFileSize },
            ["allowedFileExtensions"] = new JsonObject
            {
                ["type"] = new JsonArray("array", "null"), ["uniqueItems"] = true,
                ["items"] = new JsonObject { ["type"] = "string", ["enum"] = Extensions(HostExtensions()) },
            },
        },
    };

    public async Task<SiteSettingsSectionResponse> GetAsync() => Describe(await _siteService.GetSettingsAsync<MediaUploadSettings>());

    public async Task<SiteSettingsSectionUpdateResult> UpdateAsync(JsonObject values)
    {
        if (values is null) { return new() { Errors = new() { ["body"] = ["A settings object is required."] } }; }
        var site = await _siteService.LoadSiteSettingsAsync();
        var current = site.GetOrCreate<MediaUploadSettings>();
        var proposed = new MediaUploadSettings { MaxFileSize = current.MaxFileSize, AllowedFileExtensions = current.AllowedFileExtensions?.ToArray() };
        var errors = new Dictionary<string, string[]>();
        foreach (var entry in values)
        {
            switch (entry.Key)
            {
                case "maxFileSize":
                    if (entry.Value is null) { proposed.MaxFileSize = null; }
                    else if (entry.Value is JsonValue number && number.TryGetValue<long>(out var limit) && limit >= 1 && limit <= _hostOptions.MaxFileSize)
                    {
                        proposed.MaxFileSize = limit;
                    }
                    else { errors[entry.Key] = ["Provide a positive size no larger than the host maximum, or null to inherit it."]; }
                    break;
                case "allowedFileExtensions":
                    if (entry.Value is null) { proposed.AllowedFileExtensions = null; }
                    else if (entry.Value is JsonArray array)
                    {
                        var extensions = new List<string>();
                        var allowed = HostExtensions().ToHashSet(StringComparer.OrdinalIgnoreCase);
                        foreach (var item in array)
                        {
                            if (item is not JsonValue text || !text.TryGetValue<string>(out var extension) || string.IsNullOrEmpty(extension)
                                || !allowed.Contains(extension))
                            {
                                errors[entry.Key] = ["Provide an array containing only extensions permitted by the host."];
                                break;
                            }
                            extensions.Add(extension.ToLowerInvariant());
                        }
                        proposed.AllowedFileExtensions = extensions.Distinct(StringComparer.OrdinalIgnoreCase).Order(StringComparer.Ordinal).ToArray();
                    }
                    else { errors[entry.Key] = ["Provide an array of extensions or null to inherit the host lists."]; }
                    break;
                default:
                    errors[entry.Key] = ["This property is not writable through the media upload policy."];
                    break;
            }
        }
        if (errors.Count > 0) { return new() { Errors = errors }; }
        var changed = proposed.MaxFileSize != current.MaxFileSize
            || !EqualExtensions(proposed.AllowedFileExtensions, current.AllowedFileExtensions);
        if (changed)
        {
            site.Put(proposed);
            await _siteService.UpdateSiteSettingsAsync(site);
            _release.RequestRelease();
        }
        return new() { Section = Describe(proposed), Changed = changed, ReloadRequested = changed };
    }

    private SiteSettingsSectionResponse Describe(MediaUploadSettings settings)
    {
        var effective = new MediaOptions
        {
            MaxFileSize = _hostOptions.MaxFileSize,
            AllowedFileExtensions = new HashSet<string>(_hostOptions.AllowedFileExtensions, StringComparer.OrdinalIgnoreCase),
            RestrictedFileExtensions = new HashSet<string>(_hostOptions.RestrictedFileExtensions, StringComparer.OrdinalIgnoreCase),
        };
        MediaUploadOptionsConfiguration.Apply(effective, settings);
        return new()
        {
            Name = "media-upload-policy", Source = "mixed",
            ReadOnlyProperties = ["hostMaxFileSize", "hostAllowedFileExtensions", "effectiveMaxFileSize", "effectiveAllowedFileExtensions", "effectiveRestrictedFileExtensions"],
            Values = new JsonObject
            {
                ["maxFileSize"] = settings.MaxFileSize, ["allowedFileExtensions"] = Extensions(settings.AllowedFileExtensions),
                ["hostMaxFileSize"] = _hostOptions.MaxFileSize, ["hostAllowedFileExtensions"] = Extensions(HostExtensions()),
                ["effectiveMaxFileSize"] = effective.MaxFileSize, ["effectiveAllowedFileExtensions"] = Extensions(effective.AllowedFileExtensions),
                ["effectiveRestrictedFileExtensions"] = Extensions(effective.RestrictedFileExtensions),
            },
        };
    }

    private static bool EqualExtensions(string[] left, string[] right) => left is null || right is null
        ? left is null && right is null
        : left.Order(StringComparer.OrdinalIgnoreCase).SequenceEqual(right.Order(StringComparer.OrdinalIgnoreCase), StringComparer.OrdinalIgnoreCase);

    private IEnumerable<string> HostExtensions() => _hostOptions.AllowedFileExtensions.Concat(_hostOptions.RestrictedFileExtensions).Distinct(StringComparer.OrdinalIgnoreCase);
    private static JsonArray Extensions(IEnumerable<string> values) => values is null ? null : new JsonArray(values.Order(StringComparer.Ordinal).Select(value => JsonValue.Create(value)).ToArray<JsonNode>());
}
