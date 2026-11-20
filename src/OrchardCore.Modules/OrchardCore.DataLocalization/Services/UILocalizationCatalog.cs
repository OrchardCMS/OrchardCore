using System.Reflection;
using OrchardCore.DataLocalization.Models;
using OrchardCore.Modules;

namespace OrchardCore.DataLocalization.Services;

/// <summary>Discovers the build-time POT resources of the application and its referenced assemblies.</summary>
internal sealed class UILocalizationCatalog
{
    private readonly Lazy<IReadOnlyList<UILocalizationResource>> _resources;

    public UILocalizationCatalog(IApplicationContext applicationContext)
    {
        _resources = new(() =>
            Discover(applicationContext.Application.Modules.Select(module => module.Assembly).Append(applicationContext.Application.Assembly)));
    }

    internal UILocalizationCatalog(IReadOnlyList<UILocalizationResource> resources)
    {
        _resources = new(resources);
    }

    /// <summary>Gets the catalog entries, ordered by assembly, context and key.</summary>
    public IReadOnlyList<UILocalizationResource> GetResources() => _resources.Value;

    /// <summary>Discovers catalogs in the given assemblies and their assembly references.</summary>
    /// <param name="assemblies">The application and module assemblies.</param>
    public static IReadOnlyList<UILocalizationResource> Discover(IEnumerable<Assembly> assemblies)
    {
        var resources = new List<UILocalizationResource>();
        var visited = new HashSet<string>(StringComparer.Ordinal);
        var queue = new Queue<Assembly>(assemblies);
        while (queue.TryDequeue(out var assembly))
        {
            if (!visited.Add(assembly.FullName))
            {
                continue;
            }

            var name = assembly.GetName().Name;
            using var stream = assembly.GetManifestResourceStream(name + ".Localization.pot");
            if (stream != null)
            {
                using var reader = new StreamReader(stream);
                resources.AddRange(PortableObjectFormat.ReadResources(reader, name));
            }

            foreach (var reference in assembly.GetReferencedAssemblies())
            {
                queue.Enqueue(Assembly.Load(reference));
            }
        }

        foreach (var group in resources.GroupBy(resource => (resource.Context, resource.Key)))
        {
            if (group.Select(resource => resource.Plural).Distinct(StringComparer.Ordinal).Count() > 1)
            {
                throw new InvalidOperationException("Conflicting embedded plural definitions for " + group.Key);
            }
        }

        return resources.OrderBy(resource => resource.AssemblyName, StringComparer.Ordinal)
            .ThenBy(resource => resource.Context, StringComparer.Ordinal).ThenBy(resource => resource.Key, StringComparer.Ordinal).ToArray();
    }
}
