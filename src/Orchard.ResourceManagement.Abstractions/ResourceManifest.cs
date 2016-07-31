using System;
using System.Collections.Generic;

namespace Orchard.ResourceManagement
{
    public class ResourceManifest
    {
        private string _basePath = "";
        private readonly IDictionary<string, IDictionary<string, ResourceDefinition>> _resources = new Dictionary<string, IDictionary<string, ResourceDefinition>>(StringComparer.OrdinalIgnoreCase);

        public virtual ResourceDefinition DefineResource(string resourceType, string resourceName)
        {
            var definition = new ResourceDefinition(this, resourceType, resourceName);
            var resources = GetResources(resourceType);
            resources[resourceName] = definition;
            return definition;
        }

        public ResourceDefinition DefineScript(string name)
        {
            return DefineResource("script", name);
        }

        public ResourceDefinition DefineStyle(string name)
        {
            return DefineResource("stylesheet", name);
        }

        public virtual IDictionary<string, ResourceDefinition> GetResources(string resourceType)
        {
            IDictionary<string, ResourceDefinition> resources;
            if (!_resources.TryGetValue(resourceType, out resources))
            {
                _resources[resourceType] = resources = new Dictionary<string, ResourceDefinition>(StringComparer.OrdinalIgnoreCase);
            }
            return resources;
        }

        public string BasePath
        {
            get
            {
                return _basePath;
            }
        }
    }

}
