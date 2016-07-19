using System.Collections.Generic;

namespace Orchard.ResourceManagement
{
    public interface IResourceManifest
    {
        ResourceDefinition DefineResource(string resourceType, string resourceName);
        string Name { get; }
        string BasePath { get; }
        IDictionary<string, ResourceDefinition> GetResources(string resourceType);
    }
}
