using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
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
                if (!GetApplication(environment).ModuleNames.Contains(name, StringComparer.Ordinal))
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
        private const string ModuleAssetsMap = "module.assets.map";
        private const string RootWithTrailingSlash = ".Modules/";

        private readonly IDictionary<string, IFileInfo> _fileInfos = new Dictionary<string, IFileInfo>();
        private readonly IFileProvider _fileProvider;

        public Module(string name)
        {
            if (!string.IsNullOrWhiteSpace(name))
            {
                Name = name;
                Path = RootWithTrailingSlash + Name;
                Assembly = Assembly.Load(new AssemblyName(name));
                _fileProvider = new EmbeddedFileProvider(Assembly);

                Assets = _fileProvider.GetFileInfo(ModuleAssetsMap).ReadAllLines().Select(a => new ModuleAsset(a));
                AssetPaths = Assets.Select(a => a.ModulePath);
            }
            else
            {
                Name = Path = string.Empty;
                Assets = Enumerable.Empty<ModuleAsset>();
                AssetPaths = Enumerable.Empty<string>();
            }
        }

        public string Name { get; }
        public string Path { get; }
        public Assembly Assembly { get; }
        public IEnumerable<ModuleAsset> Assets { get; }
        public IEnumerable<string> AssetPaths { get; }

        public IFileInfo GetFileInfo(string subpath)
        {
            if (!_fileInfos.TryGetValue(subpath, out var fileInfo))
            {
                if (!AssetPaths.Contains(subpath, StringComparer.Ordinal))
                {
                    return new NotFoundFileInfo(subpath);
                }

                lock (_fileInfos)
                {
                    if (!_fileInfos.TryGetValue(subpath, out fileInfo))
                    {
                        var fileName = subpath.Substring(Path.Length + 1);
                        _fileInfos[subpath] = fileInfo = _fileProvider.GetFileInfo(fileName);
                    }
                }
            }

            return fileInfo;
        }
    }

    public class ModuleAsset
    {
        public ModuleAsset(string asset)
        {
            asset = asset.Replace('\\', '/');
            var index = asset.IndexOf('|');

            if (index == -1)
            {
                ModulePath = asset;
                ProjectPath = string.Empty;
            }
            else
            {
                ModulePath = asset.Substring(0, index);
                ProjectPath = asset.Substring(index + 1);
            }
        }

        public string ModulePath { get;  }
        public string ProjectPath { get; }
    }
}
