using System.Text.Json.Nodes;
using Microsoft.Extensions.Options;
using OrchardCore.Entities;
using OrchardCore.Environment.Options;
using OrchardCore.Security.Permissions;
using OrchardCore.Settings;

namespace OrchardCore.Email.Services;

internal sealed class EmailSettingsSectionProvider : ISiteSettingsSectionProvider
{
    private readonly ISiteService _siteService;
    private readonly IOptionsMonitor<EmailProviderOptions> _providers;
    private readonly EmailSettingsEditor _editor;

    public EmailSettingsSectionProvider(ISiteService siteService,
        IOptionsMonitor<EmailProviderOptions> providers, IOptionsUpdateNotifier notifier)
    {
        _siteService = siteService;
        _providers = providers;
        _editor = new EmailSettingsEditor(providers, notifier);
    }

    public SiteSettingsSectionDescriptor Descriptor { get; } = new()
    {
        Name = "email", DisplayName = "Email", FeatureId = "OrchardCore.Email",
        Description = "Default email provider selection. Omission retains selection; null restores provider fallback. Provider credentials are managed separately.",
    };
    public Permission ReadPermission => EmailPermissions.ManageEmailSettings;
    public Permission UpdatePermission => EmailPermissions.ManageEmailSettings;

    public JsonObject GetSchema() => new()
    {
        ["$schema"] = "https://json-schema.org/draft/2020-12/schema",
        ["type"] = "object", ["additionalProperties"] = false,
        ["properties"] = new JsonObject
        {
            ["defaultProvider"] = new JsonObject { ["type"] = new JsonArray("string", "null") },
        },
    };

    public async Task<SiteSettingsSectionResponse> GetAsync()
    {
        var site = await _siteService.GetSiteSettingsAsync();
        return Describe(site.GetOrCreate<EmailSettings>());
    }

    public async Task<SiteSettingsSectionUpdateResult> UpdateAsync(JsonObject values)
    {
        var errors = new Dictionary<string, string[]>();
        if (values is null)
        {
            return new() { Errors = new() { ["body"] = ["A settings object is required."] } };
        }
        var site = await _siteService.LoadSiteSettingsAsync();
        var settings = site.GetOrCreate<EmailSettings>();
        var selected = settings.DefaultProviderName;
        foreach (var entry in values)
        {
            if (entry.Key != "defaultProvider")
            {
                errors[entry.Key] = ["This property is not managed by the email section."];
            }
            else if (entry.Value is null)
            {
                selected = null;
            }
            else if (entry.Value is JsonValue node && node.TryGetValue<string>(out var name))
            {
                selected = name;
            }
            else
            {
                errors[entry.Key] = ["Provide a provider name or null for fallback."];
            }
        }
        if (!_editor.IsAvailable(selected))
        {
            errors["defaultProvider"] = ["Select an enabled email provider."];
        }
        if (errors.Count > 0)
        {
            return new() { Errors = errors };
        }
        var changed = _editor.Apply(settings, selected);
        if (changed)
        {
            site.Put(settings);
            await _siteService.UpdateSiteSettingsAsync(site);
        }
        return new() { Changed = changed, Section = Describe(settings) };
    }

    private SiteSettingsSectionResponse Describe(EmailSettings settings) => new()
    {
        Name = "email", Source = "tenant",
        ReadOnlyProperties = ["availableProviders"],
        Values = new JsonObject
        {
            ["defaultProvider"] = settings.DefaultProviderName,
            ["availableProviders"] = new JsonArray(_providers.CurrentValue.Providers
                .Where(entry => entry.Value.IsEnabled).Select(entry => entry.Key).Order(StringComparer.Ordinal)
                .Select(name => (JsonNode)JsonValue.Create(name)).ToArray()),
        },
    };
}
