using Microsoft.Framework.Runtime;

namespace OrchardVNext.Environment {
    public abstract class HostEnvironment : IHostEnvironment {
        private readonly IApplicationEnvironment _applicationEnvrionment;
        public HostEnvironment(IApplicationEnvironment applicationEnvrionment) {
            _applicationEnvrionment = applicationEnvrionment;
        }

        public string MapPath(string virtualPath) {
            return _applicationEnvrionment.ApplicationBasePath + virtualPath.Replace("~", string.Empty).Replace('/', '\\');
        }

        public bool IsAssemblyLoaded(string name) {

            Logger.Error("TODO: Check Assembly Is Loaded");
            return false;// AppDomain.CurrentDomain.GetAssemblies().Any(assembly => new AssemblyName(assembly.FullName).Name == name);
        }

        public abstract void RestartAppDomain();
    }
}