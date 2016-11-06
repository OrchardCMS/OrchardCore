namespace Orchard.Environment.Extensions.Features
{
    public class NotFoundFeatureInfo : IFeatureInfo
    {
        private readonly string _id;
        private readonly string _name;
        private readonly double _priority;
        private readonly IExtensionInfo _extension;
        private readonly string[] _dependencies;

        public NotFoundFeatureInfo(
            string id,
            IExtensionInfo extensionInfo)
        {
            _id = id;
            _name = id;
            _priority = 0D;
            _extension = extensionInfo;
            _dependencies = new string[0];
        }

        public string Id { get { return _id; } }
        public string Name { get { return _name; } }
        public double Priority { get { return _priority; } }
        public IExtensionInfo Extension { get { return _extension; } }
        public string[] Dependencies { get { return _dependencies; } }
    }
}
