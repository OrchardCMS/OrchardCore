using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement.Extensions;
using OrchardCore.Environment.Extensions;
using OrchardCore.Environment.Shell;
using OrchardCore.Templates.Models;

namespace OrchardCore.Templates.Services;

/// <summary>
/// Lists the templates of the enabled modules, all of them or those of one module, marking those that the active theme
/// or another module overrides. They are almost all Razor templates, listed as a reference to override them.
/// </summary>
public sealed class FeatureTemplateSource : ITemplateSource
{
    public const string SourceName = "Feature";

    /// <summary>
    /// The name of the scope listing the templates of all the features.
    /// </summary>
    public const string AllFeaturesScope = "all";

    private readonly ShapeTemplateFiles _shapeTemplateFiles;
    private readonly IShellFeaturesManager _shellFeaturesManager;

    private readonly ActiveThemes _activeThemes;

    internal readonly IStringLocalizer S;

    public FeatureTemplateSource(
        ShapeTemplateFiles shapeTemplateFiles,
        IShellFeaturesManager shellFeaturesManager,
        ActiveThemes activeThemes,
        IStringLocalizer<FeatureTemplateSource> stringLocalizer)
    {
        _shapeTemplateFiles = shapeTemplateFiles;
        _shellFeaturesManager = shellFeaturesManager;
        _activeThemes = activeThemes;
        S = stringLocalizer;
    }

    public string Name => SourceName;

    public LocalizedString DisplayName => S["Feature Templates"];

    public LocalizedString GetDescription(bool adminTemplates) => adminTemplates
        ? S["The templates of the enabled features. They are Razor templates, which can be viewed as a reference to write an admin template overriding them. A template overridden by the admin theme or another feature is not the one rendered."]
        : S["The templates of the enabled features. They are Razor templates, which can be viewed as a reference to write a template overriding them. A template overridden by the site theme or another feature is not the one rendered."];

    public async Task<IEnumerable<TemplateSourceScope>> GetScopesAsync(bool adminTemplates)
    {
        var scopes = new List<TemplateSourceScope>
        {
            new() { Name = AllFeaturesScope, DisplayName = S["All features"], IsInUse = true },
        };

        foreach (var module in (await GetModulesAsync()).OrderBy(module => module.Manifest?.Name ?? module.Id, StringComparer.OrdinalIgnoreCase))
        {
            if (_shapeTemplateFiles.GetExtensionTemplates([module.Id]).Count > 0)
            {
                scopes.Add(new TemplateSourceScope { Name = module.Id, DisplayName = module.Manifest?.Name ?? module.Id, IsInUse = true });
            }
        }

        return scopes;
    }

    public async Task<IEnumerable<SourceTemplate>> GetTemplatesAsync(bool adminTemplates, string scope)
    {
        var modules = await GetModulesAsync();

        if (string.Equals(scope, AllFeaturesScope, StringComparison.OrdinalIgnoreCase))
        {
            scope = null;
        }

        // The module templates by shape, of the modules in the order of the enabled features, so that the last one,
        // a module depending on the others, wins, as it does in the shape table.
        var templatesByModule = modules.ToDictionary(
            module => module.Id,
            module => _shapeTemplateFiles.GetExtensionTemplates([module.Id]),
            StringComparer.OrdinalIgnoreCase);

        if (!string.IsNullOrEmpty(scope) && !templatesByModule.ContainsKey(scope))
        {
            return [];
        }

        var winners = new Dictionary<string, SourceTemplate>(StringComparer.OrdinalIgnoreCase);

        foreach (var module in modules)
        {
            foreach (var template in templatesByModule[module.Id])
            {
                winners[template.Name] = template;
            }
        }

        // The templates rendering the shapes with the active theme, which overrides the module templates.
        var renderedBy = await _shapeTemplateFiles.GetRenderingExtensionsAsync(await _activeThemes.GetThemeIdAsync(adminTemplates));

        // All the features list one template by shape, the one of the module that wins.
        var templates = string.IsNullOrEmpty(scope)
            ? winners.Values.ToList()
            : templatesByModule[scope];

        foreach (var template in templates)
        {
            template.IsAdmin = SourceTemplateNames.IsAdminTemplate(template.Name);

            if (renderedBy.TryGetValue(template.Name, out var extensionId) && !string.Equals(extensionId, template.ExtensionId, StringComparison.OrdinalIgnoreCase))
            {
                template.OverriddenBy = extensionId;
            }
            else if (!string.Equals(winners[template.Name].ExtensionId, template.ExtensionId, StringComparison.OrdinalIgnoreCase))
            {
                template.OverriddenBy = winners[template.Name].ExtensionId;
            }
        }

        return templates;
    }

    // The modules of the enabled features, not the themes, in the order of the features.
    private async Task<List<IExtensionInfo>> GetModulesAsync()
        => (await _shellFeaturesManager.GetEnabledFeaturesAsync())
            .Select(feature => feature.Extension)
            .Where(extension => extension != null && extension.Exists && extension.Manifest?.IsTheme() != true)
            .DistinctBy(extension => extension.Id, StringComparer.OrdinalIgnoreCase)
            .ToList();

}
