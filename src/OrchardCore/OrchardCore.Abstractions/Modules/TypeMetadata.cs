using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Annotype;

namespace OrchardCore.Modules;

/// <summary>
/// Reads the type metadata that Annotype captures from the attributes required by <see cref="RequiresMetadataAttribute"/>
/// contracts, without reflection and without creating instances of the annotated types.
/// </summary>
/// <remarks>
/// The registry that Annotype generates in an assembly is registered the first time metadata is read from one of its
/// types, or when the assembly is loaded as a module.
/// </remarks>
public static class TypeMetadata
{
    private const string GeneratedNamespace = "Annotype.Generated";

    private static readonly ConcurrentDictionary<Assembly, bool> s_registeredAssemblies = new();

    /// <summary>
    /// Returns the <typeparamref name="TAttribute"/> metadata declared on <paramref name="type"/>.
    /// </summary>
    /// <typeparam name="TAttribute">The required metadata attribute type.</typeparam>
    /// <param name="type">The annotated type.</param>
    /// <returns>A new attribute instance on every call.</returns>
    /// <exception cref="MetadataNotFoundException">The metadata is not declared on <paramref name="type"/>.</exception>
    public static TAttribute Get<TAttribute>(Type type)
        where TAttribute : Attribute
    {
        ArgumentNullException.ThrowIfNull(type);

        Register(type.Assembly);

        return (TAttribute)Metadata.Get(type, typeof(TAttribute));
    }

    /// <summary>
    /// Attempts to return the <typeparamref name="TAttribute"/> metadata declared on <paramref name="type"/>.
    /// </summary>
    /// <typeparam name="TAttribute">The required metadata attribute type.</typeparam>
    /// <param name="type">The annotated type.</param>
    /// <param name="metadata">A new attribute instance, or <see langword="null"/> when the metadata is not declared.</param>
    /// <returns><see langword="true"/> if the metadata was found.</returns>
    public static bool TryGet<TAttribute>(Type type, [NotNullWhen(true)] out TAttribute metadata)
        where TAttribute : Attribute
    {
        ArgumentNullException.ThrowIfNull(type);

        Register(type.Assembly);

        if (Metadata.TryGet(type, typeof(TAttribute), out var value))
        {
            metadata = (TAttribute)value;
            return true;
        }

        metadata = null;
        return false;
    }

    /// <summary>
    /// Registers the generated metadata registry of an assembly, if it has one.
    /// </summary>
    /// <param name="assembly">The assembly to register the metadata registry of.</param>
    /// <returns><see langword="true"/> if the assembly has a metadata registry.</returns>
    /// <remarks>
    /// The registry is found by its name, which Annotype derives from the assembly name by replacing dots with
    /// underscores. Registering the same assembly more than once has no effect.
    /// </remarks>
    public static bool Register(Assembly assembly)
    {
        ArgumentNullException.ThrowIfNull(assembly);

        return s_registeredAssemblies.GetOrAdd(assembly, static assembly =>
        {
            var assemblyName = assembly.GetName().Name;

            if (string.IsNullOrEmpty(assemblyName))
            {
                return false;
            }

            var registryType = assembly.GetType($"{GeneratedNamespace}.{assemblyName.Replace('.', '_')}.MetadataRegistry", throwOnError: false);

            if (registryType?.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static)?.GetValue(null) is not IMetadataRegistry registry)
            {
                return false;
            }

            Metadata.Register(registry);

            return true;
        });
    }
}
