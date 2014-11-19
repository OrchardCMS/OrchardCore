using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using OrchardVNext.FileSystems.VirtualPath;
using System;
using OrchardVNext.Exceptions;
using Microsoft.CodeAnalysis;

namespace OrchardVNext.Environment {
    public interface IBuildManager : IDependency {
        IEnumerable<Assembly> GetReferencedAssemblies();
        bool HasReferencedAssembly(string name);
        Assembly GetReferencedAssembly(string name);
        Assembly GetCompiledAssembly(string virtualPath);
    }

    public class DefaultBuildManager : IBuildManager {
        private readonly IVirtualPathProvider _virtualPathProvider;
        private readonly IAssemblyLoader _assemblyLoader;

        public DefaultBuildManager(
            IVirtualPathProvider virtualPathProvider, 
            IAssemblyLoader assemblyLoader) {

            _virtualPathProvider = virtualPathProvider;
            _assemblyLoader = assemblyLoader;
        }

        public IEnumerable<Assembly> GetReferencedAssemblies() {
            return null;// BuildManager.GetReferencedAssemblies().OfType<Assembly>();
        }

        public bool HasReferencedAssembly(string name) {
            var assemblyPath = _virtualPathProvider.Combine("~/bin", name + ".dll");
            return _virtualPathProvider.FileExists(assemblyPath);
        }

        public Assembly GetReferencedAssembly(string name) {
            if (!HasReferencedAssembly(name))
                return null;

            return _assemblyLoader.Load(name);
        }


        public Assembly GetCompiledAssembly(string virtualPath) {
            try {
                return null;
            }
            catch (Exception ex) {
                if (ex.IsFatal()) throw;

                Logger.Warning(ex, "Error when compiling assembly under {0}.", virtualPath);
                return null;
            }

        }
    }
}