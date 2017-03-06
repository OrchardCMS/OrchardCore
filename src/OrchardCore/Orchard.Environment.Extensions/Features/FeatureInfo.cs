namespace Orchard.Environment.Extensions.Features
{
    public class FeatureInfo : IFeatureInfo
    {
        private readonly string _id;
        private readonly string _name;
        private readonly int _priority;
        private readonly string _category;
        private readonly string _description;
        private readonly IExtensionInfo _extension;
        private readonly string[] _dependencies;

        public FeatureInfo(
            string id,
            string name,
            int priority,
            string category,
            string description,
            IExtensionInfo extension,
            string[] dependencies)
        {
            _id = id;
            _name = name;
            _priority = priority;
            _category = category;
            _description = description;
            _extension = extension;
            _dependencies = dependencies;
        }

        public string Id { get { return _id; } }
        public string Name { get { return _name; } }
        public int Priority { get { return _priority; } }
        public string Category { get { return _category; } }
        public string Description { get { return _description; } }
        public IExtensionInfo Extension { get { return _extension; } }
        public string[] Dependencies { get { return _dependencies; } }
    }
}
