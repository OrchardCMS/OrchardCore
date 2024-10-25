using System.Collections.Specialized;

namespace OrchardCore.ResourceManagement;

#pragma warning disable CA1010 // Type 'ResourceDictionary' directly or indirectly inherits 'ICollection' without implementing any of 'ICollection<T>', 'IReadOnlyCollection<T>'. Publicly-visible types should implement the generic version to broaden usability.
public class ResourceDictionary : OrderedDictionary
#pragma warning restore CA1010
{
    private readonly Stack<ResourceDefinition> _expanding = new();

    public int FirstCount { get; private set; }
    public int LastCount { get; private set; }

    public void AddExpandingResource(ResourceDefinition resource, RequireSettings settings)
    {
        if (_expanding.Contains(resource))
        {
            throw new InvalidOperationException($"Circular dependency of type '{settings.Type}' detected between '{settings.Name}' and '{resource.Name}'");
        }

        _expanding.Push(resource);
    }

    public void AddExpandedResource(ResourceDefinition resource, RequireSettings settings)
    {
        _expanding.Pop();

        if (settings.Position != ResourcePosition.ByDependency)
        {
            var existing = (RequireSettings)this[resource];
            if (existing == null || existing.Position == ResourcePosition.ByDependency)
            {
                if (settings.Position == ResourcePosition.First)
                {
                    FirstCount++;
                }

                if (settings.Position == ResourcePosition.Last)
                {
                    LastCount++;
                }
            }
        }

        this[resource] = settings;
    }
}
