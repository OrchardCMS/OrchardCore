using System.Reflection;
using System.Runtime.Loader;

namespace OrchardCore.Localization.Extraction;

/// <summary>
/// Binds the extraction to the Roslyn and Razor compiler assemblies of the .NET SDK that drives the build, instead
/// of the versions the tooling was compiled against. The SDK's Razor compiler requires the SDK's Roslyn, and both
/// evolve with every SDK release, so the tooling only compiles against a minimum Roslyn version and loads the
/// SDK's newer assemblies at run time.
/// </summary>
public static class SdkAssemblyResolver
{
    private static readonly string[] s_roslynAssemblies = ["Microsoft.CodeAnalysis", "Microsoft.CodeAnalysis.CSharp"];
    private static readonly Lock s_sync = new();
    private static readonly List<string> s_directories = [];
    private static bool s_registered;

    /// <summary>
    /// Registers directories whose assemblies take precedence over the ones deployed with the tooling.
    /// The Roslyn assemblies found there are loaded immediately, so that every later reference binds to them,
    /// and any other assembly is resolved from these directories when it cannot be found elsewhere.
    /// Call this before any compiler type is used, typically from the entry point of the process.
    /// </summary>
    /// <param name="directories">The directories to probe, such as the SDK's <c>Roslyn/bincore</c> and <c>Sdks/Microsoft.NET.Sdk.Razor/source-generators</c> directories. Null, empty, and missing directories are ignored.</param>
    /// <exception cref="InvalidOperationException">Thrown when a Roslyn assembly was already loaded from another location.</exception>
    public static void Register(params string?[] directories)
    {
        lock (s_sync)
        {
            if (!s_registered)
            {
                s_registered = true;
                AssemblyLoadContext.Default.Resolving += Resolve;
            }

            foreach (var directory in directories)
            {
                if (string.IsNullOrEmpty(directory) || !Directory.Exists(directory))
                {
                    continue;
                }

                var fullPath = Path.GetFullPath(directory);
                if (s_directories.Contains(fullPath, StringComparer.Ordinal))
                {
                    continue;
                }

                s_directories.Add(fullPath);
                foreach (var name in s_roslynAssemblies)
                {
                    var path = Path.Combine(fullPath, name + ".dll");
                    if (!File.Exists(path))
                    {
                        continue;
                    }

                    try
                    {
                        AssemblyLoadContext.Default.LoadFromAssemblyPath(path);
                    }
                    catch (FileLoadException exception)
                    {
                        throw new InvalidOperationException($"The compiler assembly '{name}' was already loaded before the SDK directory '{fullPath}' was registered.", exception);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Gets the version and location of the Roslyn compiler the process is bound to.
    /// </summary>
    public static string Describe()
    {
        var assembly = typeof(Microsoft.CodeAnalysis.Compilation).Assembly;

        return $"Roslyn {assembly.GetName().Version} ({assembly.Location})";
    }

    private static Assembly? Resolve(AssemblyLoadContext context, AssemblyName name)
    {
        if (string.IsNullOrEmpty(name.Name))
        {
            return null;
        }

        string[] directories;
        lock (s_sync)
        {
            directories = [.. s_directories];
        }

        foreach (var directory in directories)
        {
            var path = Path.Combine(directory, name.Name + ".dll");
            if (File.Exists(path))
            {
                return context.LoadFromAssemblyPath(path);
            }
        }

        return null;
    }
}
