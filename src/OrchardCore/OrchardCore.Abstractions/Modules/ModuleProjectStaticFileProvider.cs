using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.FileProviders.Physical;
using Microsoft.Extensions.Primitives;
using OrchardCore.Modules.FileProviders;

namespace OrchardCore.Modules;

/// <summary>
/// This custom <see cref="IFileProvider"/> implementation provides the file contents of files
/// whose path is under a Module Project 'wwwroot' folder, and while in a development environment.
/// </summary>
public class ModuleProjectStaticFileProvider : IModuleStaticFileProvider
{
    private static Dictionary<string, string> _roots;
    private static readonly object _synLock = new();

    public ModuleProjectStaticFileProvider(IApplicationContext applicationContext)
    {
        if (_roots != null)
        {
            return;
        }

        lock (_synLock)
        {
            if (_roots == null)
            {
                var application = applicationContext.Application;

                var roots = new Dictionary<string, string>();

                // Resolve all module projects "wwwroot".
                foreach (var module in application.Modules)
                {
                    // If the module and the application assemblies are not at the same location,
                    // this means that the module is referenced as a package, not as a project in dev.
                    if (module.Assembly == null || Path.GetDirectoryName(module.Assembly.Location)
                        != Path.GetDirectoryName(application.Assembly.Location))
                    {
                        continue;
                    }

                    // Get the 1st module asset under "Areas/{ModuleId}/wwwroot/".
                    var asset = module.Assets.FirstOrDefault(a => a.ModuleAssetPath
                        .StartsWith(module.Root + Module.WebRoot, StringComparison.Ordinal));

                    if (asset != null)
                    {
                        // Resolve "{ModuleProjectDirectory}wwwroot/" from the project asset.
                        var index = asset.ProjectAssetPath.IndexOf('/' + Module.WebRoot, StringComparison.Ordinal);

                        // Add the module project "wwwroot" folder, canonicalized and with a trailing
                        // separator so that resolved file paths can be checked for containment.
                        var root = asset.ProjectAssetPath[..(index + Module.WebRoot.Length + 1)];

                        roots[module.Name] = PhysicalPathResolver.NormalizeRoot(root);
                    }
                }

                _roots = roots;
            }
        }
    }

    public IDirectoryContents GetDirectoryContents(string subpath)
    {
        return NotFoundDirectoryContents.Singleton;
    }

    public IFileInfo GetFileInfo(string subpath)
    {
        if (subpath == null)
        {
            return new NotFoundFileInfo(subpath);
        }

        // "{ModuleId}/**/*.*".
        if (TryGetProjectFilePath(NormalizePath(subpath), out var filePath))
        {
            // Serve the file from the physical file system.
            return new PhysicalFileInfo(new FileInfo(filePath));
        }

        return new NotFoundFileInfo(subpath);
    }

    public IChangeToken Watch(string filter)
    {
        if (filter == null)
        {
            return NullChangeToken.Singleton;
        }

        // "{ModuleId}/**/*.*".
        if (TryGetProjectFilePath(NormalizePath(filter), out var filePath))
        {
            // Watch the file from the physical file system.
            return new PollingFileChangeToken(new FileInfo(filePath));
        }

        return NullChangeToken.Singleton;
    }

    /// <summary>
    /// Resolves an existing physical file path from a "{ModuleId}/**/*.*" path, only if the
    /// canonical path is contained in the module project "wwwroot" folder.
    /// </summary>
    private static bool TryGetProjectFilePath(string path, out string filePath)
    {
        filePath = null;

        var index = path.IndexOf('/');

        if (index == -1)
        {
            return false;
        }

        // Resolve the module id.
        var module = path[..index];

        // Get the module project "wwwroot" folder.
        if (!_roots.TryGetValue(module, out var root))
        {
            return false;
        }

        // Resolve "{ModuleProjectDirectory}wwwroot/**/*.*", but only inside that folder.
        if (!PhysicalPathResolver.TryResolve(root, path[(module.Length + 1)..], out var resolvedPath) ||
            !File.Exists(resolvedPath))
        {
            return false;
        }

        filePath = resolvedPath;

        return true;
    }

    private static string NormalizePath(string path) => path.Replace('\\', '/').Trim('/').Replace("//", "/");
}
