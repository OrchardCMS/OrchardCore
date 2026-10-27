using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement.Extensions;
using OrchardCore.Environment.Extensions;
using OrchardCore.Modules.Manifest;
using OrchardCore.Templates.Models;

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

    private readonly ActiveThemes _activeThemes;

    internal readonly IStringLocalizer S;

    public ThemeTemplateSource(
        ShapeTemplateFiles shapeTemplateFiles,
        IExtensionManager extensionManager,
        ActiveThemes activeThemes,
        IStringLocalizer<ThemeTemplateSource> stringLocalizer)
    {
        _shapeTemplateFiles = shapeTemplateFiles;
        _extensionManager = extensionManager;
        _activeThemes = activeThemes;
        S = stringLocalizer;
    }

    public string Name => SourceName;

    public LocalizedString DisplayName => S["Theme Templates"];

    public LocalizedString GetDescription(bool adminTemplates) => adminTemplates
        ? S["The templates of the admin themes. Clone a Liquid template of the active theme to customize it as an admin template, without changing the theme. Razor templates can be viewed as a reference."]
        : S["The templates of the site themes. Clone a Liquid template of the active theme to customize it as a template, without changing the theme. Razor templates can be viewed as a reference."];

    public async Task<IEnumerable<TemplateSourceScope>> GetScopesAsync(bool adminTemplates)
    {
        var activeThemeId = await _activeThemes.GetThemeIdAsync(adminTemplates);

        return GetThemes(adminTemplates)
            .Select(theme =>
            {
                var isActive = string.Equals(theme.Id, activeThemeId, StringComparison.OrdinalIgnoreCase);
                var name = theme.Manifest?.Name ?? theme.Id;

                return new TemplateSourceScope
                {
                    Name = theme.Id,
                    DisplayName = isActive ? S["{0} (active)", name] : name,
                    IsInUse = isActive,
                };
            })

            // The active theme first, as it is listed by default.
            .OrderByDescending(scope => scope.IsInUse)
            .ThenBy(scope => scope.DisplayName, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public async Task<IEnumerable<SourceTemplate>> GetTemplatesAsync(bool adminTemplates, string scope)
    {
        var activeThemeId = await _activeThemes.GetThemeIdAsync(adminTemplates);
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
