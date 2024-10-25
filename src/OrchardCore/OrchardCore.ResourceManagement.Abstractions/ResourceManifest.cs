namespace OrchardCore.ResourceManagement;

public class ResourceManifest
{
    private readonly Dictionary<string, IDictionary<string, IList<ResourceDefinition>>> _resources = new(StringComparer.OrdinalIgnoreCase);

    public virtual ResourceDefinition DefineResource(string resourceType, string resourceName)
    {
        var definition = new ResourceDefinition(this, resourceType, resourceName);
        var resources = GetResources(resourceType);
        if (!resources.TryGetValue(resourceName, out var value))
        {
            value = new List<ResourceDefinition>();
            resources[resourceName] = value;
        }

        value.Add(definition);
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

    public virtual IDictionary<string, IList<ResourceDefinition>> GetResources(string resourceType)
    {
        if (!_resources.TryGetValue(resourceType, out var resources))
        {
            _resources[resourceType] = resources = new Dictionary<string, IList<ResourceDefinition>>(StringComparer.OrdinalIgnoreCase);
        }

        return resources;
    }

    public string BasePath { get; } = "";
}
