using OrchardCore.Environment.Extensions;
using OrchardCore.Environment.Extensions.Features;
using OrchardCore.Environment.Extensions.Manifests;

namespace OrchardCore.DisplayManagement.Events;

/// <summary>
/// Synthesizes the main feature for theme extensions that declare explicit sub-features
/// via <see cref="Modules.Manifest.FeatureAttribute"/> but do not declare their own main feature.
/// Without this, such themes would have no feature whose Id matches the extension Id, and
/// <c>IsTheme()</c> would never return true for the extension.
/// </summary>
public class ThemeFeaturesProvider : IFeaturesProvider
{
    private readonly IEnumerable<IFeatureBuilderEvents> _featureBuilderEvents;

    public ThemeFeaturesProvider(IEnumerable<IFeatureBuilderEvents> featureBuilderEvents)
    {
        _featureBuilderEvents = featureBuilderEvents;
    }

    public IEnumerable<IFeatureInfo> GetFeatures(IExtensionInfo extensionInfo, IManifestInfo manifestInfo)
    {
        if (!manifestInfo.Type.Equals("Theme", StringComparison.OrdinalIgnoreCase))
        {
            return [];
        }

        var features = manifestInfo.ModuleInfo.Features;

        if (features.Count == 0)
        {
            return [];
        }

        if (features.Any(f => f.Id == extensionInfo.Id))
        {
            return [];
        }

        var moduleInfo = manifestInfo.ModuleInfo;

        var mainContext = new FeatureBuildingContext
        {
            FeatureId = extensionInfo.Id,
            FeatureName = manifestInfo.Name,
            Category = string.IsNullOrEmpty(moduleInfo.Category) ? "Uncategorized" : moduleInfo.Category,
            Description = moduleInfo.Description ?? "",
            ExtensionInfo = extensionInfo,
            ManifestInfo = manifestInfo,
            Priority = int.TryParse(moduleInfo.Priority, out var priority) ? priority : 0,
            FeatureDependencyIds = moduleInfo.Dependencies,
            DefaultTenantOnly = moduleInfo.DefaultTenantOnly,
            IsAlwaysEnabled = moduleInfo.IsAlwaysEnabled,
            EnabledByDependencyOnly = moduleInfo.EnabledByDependencyOnly,
        };

        foreach (var builder in _featureBuilderEvents)
        {
            builder.Building(mainContext);
        }

        var mainFeatureInfo = new ThemeFeatureInfo(
            mainContext.FeatureId,
            mainContext.FeatureName,
            mainContext.Priority,
            mainContext.Category,
            mainContext.Description,
            mainContext.ExtensionInfo,
            mainContext.FeatureDependencyIds,
            mainContext.DefaultTenantOnly,
            mainContext.IsAlwaysEnabled,
            mainContext.EnabledByDependencyOnly);

        foreach (var builder in _featureBuilderEvents)
        {
            builder.Built(mainFeatureInfo);
        }

        return [mainFeatureInfo];
    }

    private sealed class ThemeFeatureInfo : IFeatureInfo
    {
        public ThemeFeatureInfo(
            string id,
            string name,
            int priority,
            string category,
            string description,
            IExtensionInfo extension,
            string[] dependencies,
            bool defaultTenantOnly,
            bool isAlwaysEnabled,
            bool enabledByDependencyOnly)
        {
            Id = id;
            Name = name;
            Priority = priority;
            Category = category;
            Description = description;
            Extension = extension;
            Dependencies = dependencies;
            DefaultTenantOnly = defaultTenantOnly;
            IsAlwaysEnabled = isAlwaysEnabled;
            EnabledByDependencyOnly = enabledByDependencyOnly;
        }

        public string Id { get; }
        public string Name { get; }
        public int Priority { get; }
        public string Category { get; }
        public string Description { get; }
        public bool DefaultTenantOnly { get; }
        public IExtensionInfo Extension { get; }
        public string[] Dependencies { get; }
        public bool IsAlwaysEnabled { get; }
        public bool EnabledByDependencyOnly { get; }
    }
}
