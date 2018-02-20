using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.FileProviders.Embedded;
using OrchardCore.Modules.FileProviders;

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
                        _application = new Application(environment.ApplicationName);
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
        private const string ModuleNamesMap = "module.names.map";

        public Application(string application)
        {
            Name = application;
            Assembly = Assembly.Load(new AssemblyName(application));
            ModuleNames = new EmbeddedFileProvider(Assembly).GetFileInfo(ModuleNamesMap).ReadAllLines();
        }

        public string Name { get; }
        public Assembly Assembly { get; }
        public IEnumerable<string> ModuleNames { get; }
    }

    public class Module
    {
        public const string ContentPath = "wwwroot";
        public static string ContentRoot = ContentPath + "/";
        private const string ModuleAssetsMap = "module.assets.map";

        private readonly string _baseNamespace;
        private readonly DateTimeOffset _lastModified;
        private readonly IDictionary<string, IFileInfo> _fileInfos = new Dictionary<string, IFileInfo>();

        public Module(string name)
        {
            if (!string.IsNullOrWhiteSpace(name))
            {
                Name = name;
                Root = Application.ModulesRoot + Name + '/';
                Assembly = Assembly.Load(new AssemblyName(name));
                Assets = new EmbeddedFileProvider(Assembly).GetFileInfo(ModuleAssetsMap).ReadAllLines().Select(a => new Asset(a));
                AssetPaths = Assets.Select(a => a.ModuleAssetPath);
            }
            else
            {
                Name = Root = string.Empty;
                Assets = Enumerable.Empty<Asset>();
                AssetPaths = Enumerable.Empty<string>();
            }

            _baseNamespace = Name + '.';
            _lastModified = DateTimeOffset.UtcNow;
        }

        public string Name { get; }
        public string Root { get; }
        public Assembly Assembly { get; }
        public IEnumerable<Asset> Assets { get; }
        public IEnumerable<string> AssetPaths { get; }

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
