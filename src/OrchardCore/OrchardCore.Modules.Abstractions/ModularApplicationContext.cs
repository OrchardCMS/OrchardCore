using System;
using System.Collections.Concurrent;
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
        private const string ModuleNamesMap = "module.names.map";
        private const string ModuleAssetsMap = "module.assets.map";

        private static Lazy<IEnumerable<string>> _moduleNames;
        private static ConcurrentDictionary<string, Lazy<IEnumerable<string>>> _moduleAssets = new ConcurrentDictionary<string, Lazy<IEnumerable<string>>>();
        private static ConcurrentDictionary<string, Lazy<Assembly>> _assemblies = new ConcurrentDictionary<string, Lazy<Assembly>>();
        private static ConcurrentDictionary<string, Lazy<IFileInfo>> _fileInfos = new ConcurrentDictionary<string, Lazy<IFileInfo>>();

        private static Assembly Load(string assemblyName)
        {
            return _assemblies.GetOrAdd(assemblyName, (name) => new Lazy<Assembly>(() =>
            {
                return Assembly.Load(new AssemblyName(name));
            })).Value;
        }

        public static Assembly LoadApplicationAssembly(this IHostingEnvironment environment)
        {
            return _assemblies.GetOrAdd(environment.ApplicationName, (key) => new Lazy<Assembly>(() =>
            {
                return Assembly.Load(new AssemblyName(key));
            })).Value;
        }

        public static Assembly LoadModuleAssembly(this IHostingEnvironment environment, string moduleId)
        {
            if (!GetModuleNames(environment).Contains(moduleId))
            {
                return null;
            }

            return _assemblies.GetOrAdd(moduleId, (key) => new Lazy<Assembly>(() =>
            {
                return Assembly.Load(new AssemblyName(key));
            })).Value;
        }

        public static IEnumerable<string> GetModuleNames(this IHostingEnvironment environment)
        {
            if (_moduleNames == null)
            {
                _moduleNames = new Lazy<IEnumerable<string>>(() =>
                {
                    return GetFileInfo(environment.ApplicationName, ModuleNamesMap).ReadAllLines();
                });
            }

            return _moduleNames.Value;
        }

        public static IEnumerable<string> GetModuleAssets(this IHostingEnvironment environment, string moduleId)
        {
            if (!GetModuleNames(environment).Contains(moduleId))
            {
                return Enumerable.Empty<string>();
            }

            return _moduleAssets.GetOrAdd(moduleId + ModuleAssetsMap, (key) => new Lazy<IEnumerable<string>>(() =>
            {
                return GetFileInfo(moduleId, ModuleAssetsMap).ReadAllLines().Select(x => x.Replace('\\', '/'));
            })).Value;
        }

        public static IFileInfo GetModuleFileInfo(this IHostingEnvironment environment, string moduleId, string fileName)
        {
            if (!GetModuleNames(environment).Contains(moduleId))
            {
                return null;
            }

            var fileInfo = GetFileInfo(moduleId, fileName);

            if (fileInfo is NotFoundFileInfo)
            {
                var hiddenFileName = "obj/hidden/" + fileName + ".hidden.culture";
                fileInfo = GetFileInfo(moduleId, hiddenFileName);

                if (!(fileInfo is NotFoundFileInfo))
                {
                    return new EmbeddedResourceFileInfo(Load(moduleId), moduleId + '.'
                        + hiddenFileName.Replace('\\', '.').Replace('/', '.'),
                        Path.GetFileName(fileName), DateTimeOffset.UtcNow);
                }
            }

            return fileInfo;
        }

        private static IFileInfo GetFileInfo(string assemblyName, string fileName)
        {
            return _fileInfos.GetOrAdd(assemblyName + fileName, (key) => new Lazy<IFileInfo>(() =>
            {
                var assembly = Load(assemblyName);
                var fileProvider = new EmbeddedFileProvider(assembly);
                return fileProvider.GetFileInfo(fileName);
            })).Value;
        }
    }
}
