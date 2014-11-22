using System;
using System.Reflection;
using OrchardVNext.Environment;
using OrchardVNext.FileSystems.AppData;
using Microsoft.Framework.Runtime;

namespace OrchardVNext.FileSystems.Dependencies {
    public class DefaultAssemblyProbingFolder : IAssemblyProbingFolder {
        private const string BasePath = "Dependencies";
        private readonly IAppDataFolder _appDataFolder;
        private readonly IAssemblyLoadContextFactory _assemblyLoadContextFactory;

        public DefaultAssemblyProbingFolder(IAppDataFolder appDataFolder, IAssemblyLoadContextFactory assemblyLoadContextFactory) {
            _appDataFolder = appDataFolder;
            _assemblyLoadContextFactory = assemblyLoadContextFactory;
        }

        public bool AssemblyExists(string moduleName) {
            var path = PrecompiledAssemblyPath(moduleName);
            return _appDataFolder.FileExists(path);
        }

        public DateTime GetAssemblyDateTimeUtc(string moduleName) {
            var path = PrecompiledAssemblyPath(moduleName);
            return _appDataFolder.GetFileLastWriteTimeUtc(path);
        }

        public string GetAssemblyVirtualPath(string moduleName) {
            var path = PrecompiledAssemblyPath(moduleName);
            if (!_appDataFolder.FileExists(path))
                return null;

            return _appDataFolder.GetVirtualPath(path);
        }

        public Assembly LoadAssembly(string moduleName) {
            var path = PrecompiledAssemblyPath(moduleName);
            if (!_appDataFolder.FileExists(path))
                return null;

            return _assemblyLoadContextFactory.Create().Load(moduleName);
        }

        public void DeleteAssembly(string moduleName) {
            var path = PrecompiledAssemblyPath(moduleName);

            if (_appDataFolder.FileExists(path)) {
                Logger.Information("Deleting assembly for module \"{0}\" from probing directory", moduleName);
                _appDataFolder.DeleteFile(path);
            }
        }

        public void StoreAssembly(string moduleName, string fileName) {
            var path = PrecompiledAssemblyPath(moduleName);

            Logger.Information("Storing assembly file \"{0}\" for module \"{1}\"", fileName, moduleName);
            _appDataFolder.StoreFile(fileName, path);
        }

        private string PrecompiledAssemblyPath(string moduleName) {
            return _appDataFolder.Combine(BasePath, moduleName + ".dll");
        }
    }
}