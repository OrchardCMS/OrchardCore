using System;

namespace OrchardCore.Environment.Extensions.Features
{
    public class FeatureInfo : IFeatureInfo
    {
        public FeatureInfo(string id, IExtensionInfo extensionInfo)
        {
            Id = Name = id;
            Extension = extensionInfo;
            Dependencies = Array.Empty<string>();
        }

        public FeatureInfo(
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
