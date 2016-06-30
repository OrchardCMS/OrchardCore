
namespace Orchard.Environment.Extensions.Compilers
{
    internal struct ResourceSource
    {
        public ResourceSource(ResourceFile resource, string metadataName)
        {
            Resource = resource;
            MetadataName = metadataName;
        }

        public ResourceFile Resource { get; }

        public string MetadataName { get; }
    }
}
