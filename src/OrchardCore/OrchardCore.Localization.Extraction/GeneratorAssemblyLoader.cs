using System.Reflection;
using Microsoft.CodeAnalysis;

namespace OrchardCore.Localization.Extraction;

/// <summary>
/// Loads generator assemblies into the default load context, where <see cref="SdkAssemblyResolver"/> binds their
/// Roslyn dependencies to the SDK's compiler.
/// </summary>
internal sealed class GeneratorAssemblyLoader : IAnalyzerAssemblyLoader
{
    public static GeneratorAssemblyLoader Instance { get; } = new();

    public void AddDependencyLocation(string fullPath)
    {
    }

    public Assembly LoadFromPath(string fullPath) => Assembly.LoadFrom(fullPath);
}
