using System.Text.Json.Nodes;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentLocalization.Models;
using OrchardCore.Entities;
using OrchardCore.Security.Permissions;
using OrchardCore.Settings;

namespace OrchardCore.ContentLocalization.Services;

internal sealed class ContentCultureSettingsSectionProvider : ISiteSettingsSectionProvider
{
    private readonly ISiteService _siteService;
    internal readonly IStringLocalizer S;

    public ContentCultureSettingsSectionProvider(ISiteService siteService, IStringLocalizer<ContentCultureSettingsSectionProvider> localizer)
    {
        _siteService = siteService;
        S = localizer;
    }

    public SiteSettingsSectionDescriptor Descriptor { get; } = new()
    {
        Name = "content-culture-picker",
        DisplayName = "Content culture picker",
        FeatureId = "OrchardCore.ContentLocalization.ContentCulturePicker",
        Description = "Tenant cookie and homepage fallback behavior for content culture selection. Omitted properties retain their values. Cookie lifetime is host configuration.",
    };

    public Permission ReadPermission => ContentLocalizationPermissions.ManageContentCulturePicker;
    public Permission UpdatePermission => ContentLocalizationPermissions.ManageContentCulturePicker;

    public JsonObject GetSchema() => new()
    {
        ["$schema"] = "https://json-schema.org/draft/2020-12/schema",
        ["title"] = "Content culture picker settings update",
        ["type"] = "object",
        ["additionalProperties"] = false,
        ["properties"] = new JsonObject
        {
            ["setCookie"] = new JsonObject { ["type"] = "boolean", ["description"] = "Write a culture cookie when a visitor switches to a supported culture." },
            ["redirectToHomepage"] = new JsonObject { ["type"] = "boolean", ["description"] = "When the current content has no target-culture variant, try the localized homepage." },
            ["setCookieOnContentRequest"] = new JsonObject { ["type"] = "boolean", ["description"] = "Write a culture cookie when visiting localized content. This does not change how the request culture is selected." },
        },
    };

    public async Task<SiteSettingsSectionResponse> GetAsync()
    {
        var site = await _siteService.GetSiteSettingsAsync();
        return ToResponse(site.GetOrCreate<ContentCulturePickerSettings>(), site.GetOrCreate<ContentRequestCultureProviderSettings>());
    }

    public async Task<SiteSettingsSectionUpdateResult> UpdateAsync(JsonObject values)
    {
        if (values is null)
        {
            return new() { Errors = new() { ["body"] = [S["A settings object is required."]] } };
        }
        var errors = new Dictionary<string, string[]>();
        foreach (var entry in values)
        {
            if (entry.Key is not ("setCookie" or "redirectToHomepage" or "setCookieOnContentRequest"))
            {
                errors[entry.Key] = [S["This property is not managed by the content culture picker settings section."]];
            }
            else if (entry.Value is not JsonValue value || !value.TryGetValue<bool>(out _))
            {
                errors[entry.Key] = [S["Provide a Boolean value; null does not reset this property."]];
            }
        }
        if (errors.Count > 0)
        {
            return new() { Errors = errors };
        }

        var site = await _siteService.LoadSiteSettingsAsync();
        var picker = site.GetOrCreate<ContentCulturePickerSettings>();
        var request = site.GetOrCreate<ContentRequestCultureProviderSettings>();
        var proposedPicker = new ContentCulturePickerSettings
        {
            SetCookie = values["setCookie"]?.GetValue<bool>() ?? picker.SetCookie,
            RedirectToHomepage = values["redirectToHomepage"]?.GetValue<bool>() ?? picker.RedirectToHomepage,
        };
        var proposedRequest = new ContentRequestCultureProviderSettings
        {
            SetCookie = values["setCookieOnContentRequest"]?.GetValue<bool>() ?? request.SetCookie,
        };
        var pickerChanged = ContentCultureSettingsEditor.Apply(picker, proposedPicker);
        var requestChanged = ContentCultureSettingsEditor.Apply(request, proposedRequest);
        if (pickerChanged)
        {
            site.Put(nameof(ContentCulturePickerSettings), picker);
        }
        if (requestChanged)
        {
            site.Put(nameof(ContentRequestCultureProviderSettings), request);
        }
        if (pickerChanged || requestChanged)
        {
            await _siteService.UpdateSiteSettingsAsync(site);
        }
        return new() { Section = ToResponse(picker, request), Changed = pickerChanged || requestChanged };
    }

    private static SiteSettingsSectionResponse ToResponse(ContentCulturePickerSettings picker, ContentRequestCultureProviderSettings request) => new()
    {
        Name = "content-culture-picker",
        Source = "tenant",
        Values = new JsonObject
        {
            ["setCookie"] = picker.SetCookie,
            ["redirectToHomepage"] = picker.RedirectToHomepage,
            ["setCookieOnContentRequest"] = request.SetCookie,
        },
    };
}
