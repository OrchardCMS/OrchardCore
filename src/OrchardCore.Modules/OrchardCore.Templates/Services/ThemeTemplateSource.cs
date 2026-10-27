using Microsoft.Extensions.Localization;
using OrchardCore.Admin;
using OrchardCore.DisplayManagement.Extensions;
using OrchardCore.Environment.Extensions;
using OrchardCore.Templates.Models;
using OrchardCore.Themes.Services;

namespace OrchardCore.Templates.Services;

/// <summary>
/// Lists the templates of the active theme, and of the themes it is based on: the site theme for the templates,
/// the admin theme for the admin templates.
/// </summary>
public sealed class ThemeTemplateSource : ITemplateSource
{
    public const string SourceName = "Theme";

    private readonly ShapeTemplateFiles _shapeTemplateFiles;
    private readonly IExtensionManager _extensionManager;

    // Registered by the Themes and Admin features, which can be disabled.
    private readonly ISiteThemeService _siteThemeService;
    private readonly IAdminThemeService _adminThemeService;

    internal readonly IStringLocalizer S;

    public ThemeTemplateSource(
        ShapeTemplateFiles shapeTemplateFiles,
        IExtensionManager extensionManager,
        IEnumerable<ISiteThemeService> siteThemeServices,
        IEnumerable<IAdminThemeService> adminThemeServices,
        IStringLocalizer<ThemeTemplateSource> stringLocalizer)
    {
        _shapeTemplateFiles = shapeTemplateFiles;
        _extensionManager = extensionManager;
        _siteThemeService = siteThemeServices.LastOrDefault();
        _adminThemeService = adminThemeServices.LastOrDefault();
        S = stringLocalizer;
    }

    public string Name => SourceName;

    public LocalizedString DisplayName => S["Theme Templates"];

    public async Task<IEnumerable<SourceTemplate>> GetTemplatesAsync(bool adminTemplates)
    {
        var themeId = adminTemplates
            ? await (_adminThemeService?.GetAdminThemeNameAsync() ?? Task.FromResult<string>(null))
            : await (_siteThemeService?.GetSiteThemeNameAsync() ?? Task.FromResult<string>(null));

        if (string.IsNullOrEmpty(themeId))
        {
            return [];
        }

        var themeIds = GetThemeIds(themeId);
        var templates = await _shapeTemplateFiles.GetTemplatesAsync(themeId);

        return templates.Where(template => themeIds.Contains(template.ExtensionId));
    }

    // The theme and the themes it is based on.
    private HashSet<string> GetThemeIds(string themeId)
    {
        var themeIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        while (!string.IsNullOrEmpty(themeId) && themeIds.Add(themeId))
        {
            var extension = _extensionManager.GetExtension(themeId);

            if (extension == null || !extension.Exists)
            {
                break;
            }

            themeId = new ThemeExtensionInfo(extension).BaseTheme;
        }

        return themeIds;
    }
}
