namespace Orchard.Environment.Extensions.Info
{
    public class FeatureInfo : IFeatureInfo
    {
        public FeatureInfo(
            string id,
            string name,
            string[] dependencies)
        {
            Id = id;
            Name = name;
            Dependencies = dependencies;
        }

        public string Id { get; private set; }
        public string Name { get; private set; }
        public string[] Dependencies { get; private set; }
    }
}
