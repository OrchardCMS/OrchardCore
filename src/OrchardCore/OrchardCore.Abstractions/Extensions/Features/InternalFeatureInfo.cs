namespace OrchardCore.Environment.Extensions.Features
{
    public class InternalFeatureInfo : IFeatureInfo
    {
        public InternalFeatureInfo(
            string id,
            IExtensionInfo extensionInfo)
        {
            Id = id;
            Name = id;
            Priority = 0;
            Category = null;
            Description = null;
            DefaultTenantOnly = false;
            Extension = extensionInfo;
            Dependencies = new string[0];
            IsAlwaysEnabled = false;
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
    }
}
