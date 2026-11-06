using System.Reflection;
using OrchardCore.DataLocalization.Models;
using OrchardCore.Modules;

namespace OrchardCore.DataLocalization.Services;

/// <summary>Discovers the build-time POT resources of the application and its referenced assemblies.</summary>
public sealed class EmbeddedUiLocalizationCatalog : IUiLocalizationCatalog
{
    private readonly Lazy<IReadOnlyList<UiLocalizationResource>> _resources;

    /// <summary>Creates a catalog for the application and all declared modules.</summary>
    /// <param name="applicationContext">The application module registry.</param>
    public EmbeddedUiLocalizationCatalog(IApplicationContext applicationContext)
    {
        _resources = new Lazy<IReadOnlyList<UiLocalizationResource>>(() =>
            Discover(applicationContext.Application.Modules.Select(module => module.Assembly).Append(applicationContext.Application.Assembly)));
    }

    /// <inheritdoc />
    public IReadOnlyList<UiLocalizationResource> GetResources() => _resources.Value;

    /// <summary>Discovers catalogs in the given assemblies and their assembly references.</summary>
    /// <param name="assemblies">The application and module assemblies.</param>
    public static IReadOnlyList<UiLocalizationResource> Discover(IEnumerable<Assembly> assemblies)
    {
        var resources = new List<UiLocalizationResource>();
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
                resources.AddRange(UiPortableObject.Read(reader, name));
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
