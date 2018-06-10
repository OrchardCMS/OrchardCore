namespace OrchardCore.Environment.Extensions.Features
{
    public class FeatureInfo : IFeatureInfo
    {
        public FeatureInfo(
            string id,
            string name,
            int priority,
            string category,
            string description,
            IExtensionInfo extension,
            string[] dependencies)
        {
            Id = id;
            Name = name;
            Priority = priority;
            Category = category;
            Description = description;
            Extension = extension;
            Dependencies = dependencies;
        }

        public string Id { get; }
        public string Name { get; }
        public int Priority { get; }
        public string Category { get; }
        public string Description { get; }
        public IExtensionInfo Extension { get; }
        public string[] Dependencies { get; }
    }
}
