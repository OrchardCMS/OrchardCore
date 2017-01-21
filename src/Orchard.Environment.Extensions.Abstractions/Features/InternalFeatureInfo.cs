using System;

namespace Orchard.Environment.Extensions.Features
{
    public class InternalFeatureInfo : IFeatureInfo
    {
        private readonly string _id;
        private readonly string _name;
        private readonly int _priority;
        private readonly string _category;
        private readonly string _description;
        private readonly IExtensionInfo _extension;
        private readonly string[] _dependencies;

        public InternalFeatureInfo(
            string id,
            IExtensionInfo extensionInfo)
        {
            _id = id;
            _name = id;
            _priority = 0;
            _category = null;
            _description = null;
            _extension = extensionInfo;
            _dependencies = new string[0];
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
