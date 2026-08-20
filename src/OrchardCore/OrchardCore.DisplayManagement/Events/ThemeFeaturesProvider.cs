using OrchardCore.DisplayManagement.Extensions;
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
public sealed class ThemeFeaturesProvider : FeaturesProviderBase
{
    public ThemeFeaturesProvider(IEnumerable<IFeatureBuilderEvents> featureBuilderEvents)
        : base(featureBuilderEvents)
    {
    }

    public override IEnumerable<IFeatureInfo> GetFeatures(IExtensionInfo extensionInfo, IManifestInfo manifestInfo)
    {
        if (!manifestInfo.IsTheme())
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
            FeatureBeforeDependencyIds = moduleInfo.Before,
            FeatureAfterDependencyIds = moduleInfo.After,
            DefaultTenantOnly = moduleInfo.DefaultTenantOnly,
            IsAlwaysEnabled = moduleInfo.IsAlwaysEnabled,
            EnabledByDependencyOnly = moduleInfo.EnabledByDependencyOnly,
        };

        var mainFeatureInfo = BuildFeature(mainContext, ctx => new ThemeFeatureInfo(
            ctx.FeatureId,
            ctx.FeatureName,
            ctx.Priority,
            ctx.Category,
            ctx.Description,
            ctx.ExtensionInfo,
            ctx.FeatureDependencyIds,
            ctx.FeatureBeforeDependencyIds,
            ctx.FeatureAfterDependencyIds,
            ctx.DefaultTenantOnly,
            ctx.IsAlwaysEnabled,
            ctx.EnabledByDependencyOnly));

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
            string[] before,
            string[] after,
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
            Dependencies = dependencies ?? [];
            Before = before ?? [];
            After = after ?? [];
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
        public string[] Before { get; }
        public string[] After { get; }
        public bool IsAlwaysEnabled { get; }
        public bool EnabledByDependencyOnly { get; }
    }
}
