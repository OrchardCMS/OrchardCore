using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyModel;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.FileProviders.Embedded;
using OrchardCore.Modules.FileProviders;
using OrchardCore.Modules.Manifest;

namespace OrchardCore.Modules
{
    public static class ModularApplicationContext
    {
        private static Application _application;
        private static readonly IDictionary<string, Module> _modules = new Dictionary<string, Module>();
        private static readonly object _synLock = new object();

        public static Application GetApplication(this IHostingEnvironment environment)
        {
            if (_application == null)
            {
                lock (_synLock)
                {
                    if (_application == null)
                    {
                        var application = environment.ApplicationName;

                        var candidateAssemblies = DependencyContext.Default
                            .GetCandidateLibraries(new[] { application, "OrchardCore.Module.Targets" })
                            .Select(lib => new AssemblyName(lib.Name)).ToArray();

                        // Preload candidate assemblies
                        Parallel.ForEach(candidateAssemblies, new ParallelOptions { MaxDegreeOfParallelism = 8 }, (a) =>
                        {
                            Assembly.Load(a);
                        });

                        // Keep candidates marked as modules
                        var moduleNames = candidateAssemblies
                            .Where(a => Assembly.Load(a).GetCustomAttribute<ModuleMarkerAttribute>() != null)
                            .Select(a => a.Name).ToArray();

                        _application = new Application()
                        {
                            Name = application,
                            Assembly = Assembly.Load(application),
                            ModuleNames = moduleNames
                        };
                    }
                }
            }

            return _application;
        }

        public static Module GetModule(this IHostingEnvironment environment, string name)
        {
            if (!_modules.TryGetValue(name, out var module))
            {
                if (!environment.GetApplication().ModuleNames.Contains(name, StringComparer.Ordinal))
                {
                    return new Module(string.Empty);
                }

                lock (_synLock)
                {
                    if (!_modules.TryGetValue(name, out module))
                    {
                        _modules[name] = module = new Module(name);
                    }
                }
            }

            return module;
        }
    }

    public class Application
    {
        public const string ModulesPath = ".Modules";
        public static string ModulesRoot = ModulesPath + "/";

        public Application()
        {
        }

        public string Name { get; internal set; }
        public Assembly Assembly { get; internal set; }
        public IEnumerable<string> ModuleNames { get; internal set; }
    }

    public class Module
    {
        public const string ContentPath = "wwwroot";
        public static string ContentRoot = ContentPath + "/";

        private readonly string _baseNamespace;
        private readonly DateTimeOffset _lastModified;
        private readonly IDictionary<string, IFileInfo> _fileInfos = new Dictionary<string, IFileInfo>();

        public Module(string name)
        {
            if (!string.IsNullOrWhiteSpace(name))
            {
                Name = name;
                SubPath = Application.ModulesRoot + Name;
                Root = SubPath + '/';

                Assembly = Assembly.Load(new AssemblyName(name));
                var module = Assembly.GetCustomAttribute<ModuleAttribute>();
                var features = Assembly.GetCustomAttributes<Manifest.FeatureAttribute>()
                    .Where(f => !(f is ModuleAttribute));

                if (module != null)
                {
                    ModuleInfo = module;
                    ModuleInfo.Features.AddRange(features);

                    var assetsMap = Assembly.GetCustomAttribute<ModuleAssetsMapAttribute>();

                    if (assetsMap != null)
                    {
                        Assets = assetsMap.Assets.Select(a => new Asset(a)).ToArray();
                        AssetPaths = Assets.Select(a => a.ModuleAssetPath).ToArray();
                    }
                }
                else
                {
                    ModuleInfo = new ModuleAttribute { Name = Name };
                }

                ModuleInfo.Id = Name;
            }

            _baseNamespace = Name + '.';
            _lastModified = DateTimeOffset.UtcNow;
        }

        public string Name { get; } = String.Empty;
        public string Root { get; } = String.Empty;
        public string SubPath { get; } = String.Empty;
        public Assembly Assembly { get; }
        public IEnumerable<Asset> Assets { get; } = Enumerable.Empty<Asset>();
        public IEnumerable<string> AssetPaths { get; } = Enumerable.Empty<string>();
        public ModuleAttribute ModuleInfo { get; } = new ModuleAttribute();

        public IFileInfo GetFileInfo(string subpath)
        {
            if (!_fileInfos.TryGetValue(subpath, out var fileInfo))
            {
                if (!AssetPaths.Contains(Root + subpath, StringComparer.Ordinal))
                {
                    return new NotFoundFileInfo(subpath);
                }

                lock (_fileInfos)
                {
                    if (!_fileInfos.TryGetValue(subpath, out fileInfo))
                    {
                        var resourcePath = _baseNamespace + subpath.Replace('/', '>');
                        var fileName = Path.GetFileName(subpath);

                        if (Assembly.GetManifestResourceInfo(resourcePath) == null)
                        {
                            return new NotFoundFileInfo(fileName);
                        }

                        _fileInfos[subpath] = fileInfo = new EmbeddedResourceFileInfo(
                            Assembly, resourcePath, fileName, _lastModified);
                    }
                }
            }

            return fileInfo;
        }
    }

    public class Asset
    {
        public Asset(string asset)
        {
            asset = asset.Replace('\\', '/');
            var index = asset.IndexOf('|');

            if (index == -1)
            {
                ModuleAssetPath = asset;
                ProjectAssetPath = string.Empty;
            }
            else
            {
                ModuleAssetPath = asset.Substring(0, index);
                ProjectAssetPath = asset.Substring(index + 1);
            }
        }

        public string ModuleAssetPath { get;  }
        public string ProjectAssetPath { get; }
    }
}
