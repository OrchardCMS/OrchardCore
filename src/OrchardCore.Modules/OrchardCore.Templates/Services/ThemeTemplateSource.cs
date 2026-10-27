using Microsoft.Extensions.Localization;
using OrchardCore.Admin;
using OrchardCore.DisplayManagement.Extensions;
using OrchardCore.Environment.Extensions;
using OrchardCore.Modules.Manifest;
using OrchardCore.Templates.Models;
using OrchardCore.Themes.Services;

namespace OrchardCore.Templates.Services;

/// <summary>
/// Lists the templates of a theme, and of the themes it is based on: the site themes for the templates, the admin themes
/// for the admin templates. The active theme is listed by default; the templates of the other themes can only be viewed.
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

    public async Task<IEnumerable<TemplateSourceScope>> GetScopesAsync(bool adminTemplates)
    {
        var activeThemeId = await GetActiveThemeIdAsync(adminTemplates);

        return GetThemes(adminTemplates)
            .Select(theme => new TemplateSourceScope
            {
                Name = theme.Id,
                DisplayName = theme.Manifest?.Name ?? theme.Id,
                IsInUse = string.Equals(theme.Id, activeThemeId, StringComparison.OrdinalIgnoreCase),
            })
            .OrderByDescending(scope => scope.IsInUse)
            .ThenBy(scope => scope.DisplayName, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public async Task<IEnumerable<SourceTemplate>> GetTemplatesAsync(bool adminTemplates, string scope)
    {
        var activeThemeId = await GetActiveThemeIdAsync(adminTemplates);
        var themeId = string.IsNullOrEmpty(scope) ? activeThemeId : scope;

        // Only the themes of the list can be listed, e.g. not an admin theme as a site theme.
        if (string.IsNullOrEmpty(themeId) || !GetThemes(adminTemplates).Any(theme => string.Equals(theme.Id, themeId, StringComparison.OrdinalIgnoreCase)))
        {
            return [];
        }

        var isInUse = string.Equals(themeId, activeThemeId, StringComparison.OrdinalIgnoreCase);
        var templates = _shapeTemplateFiles.GetExtensionTemplates(GetThemeIds(themeId));

        foreach (var template in templates)
        {
            template.IsInUse = isInUse;
        }

        return templates;
    }

    private async Task<string> GetActiveThemeIdAsync(bool adminTemplates)
    {
        if (adminTemplates)
        {
            return _adminThemeService == null ? null : await _adminThemeService.GetAdminThemeNameAsync();
        }

        return _siteThemeService == null ? null : await _siteThemeService.GetSiteThemeNameAsync();
    }

    // The themes that can be selected as a site theme, or as an admin theme, like on the Themes page.
    private IEnumerable<IExtensionInfo> GetThemes(bool adminTemplates)
        => _extensionManager.GetExtensions().Where(extension =>
        {
            if (!extension.Exists || extension.Manifest?.IsTheme() != true ||
                extension.Features.Any(feature => feature.IsAlwaysEnabled || feature.EnabledByDependencyOnly))
            {
                return false;
            }

            var tags = extension.Manifest.Tags;

            if (tags.Any(tag => string.Equals(tag, "hidden", StringComparison.OrdinalIgnoreCase)))
            {
                return false;
            }

            return adminTemplates == tags.Any(tag => string.Equals(tag, ManifestConstants.AdminTag, StringComparison.OrdinalIgnoreCase));
        });

    // The theme and the themes it is based on, the theme first.
    private List<string> GetThemeIds(string themeId)
    {
        var themeIds = new List<string>();

        while (!string.IsNullOrEmpty(themeId) && !themeIds.Contains(themeId, StringComparer.OrdinalIgnoreCase))
        {
            var extension = _extensionManager.GetExtension(themeId);

            if (extension == null || !extension.Exists)
            {
                break;
            }

            themeIds.Add(extension.Id);
            themeId = new ThemeExtensionInfo(extension).BaseTheme;
        }

        return themeIds;
    }
}
