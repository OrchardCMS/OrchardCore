using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Http;
using System;
using System.Reflection;
using Microsoft.Dnx.Runtime;
using Microsoft.Dnx.Runtime.Loader;
using OrchardVNext.Configuration.Environment;

namespace OrchardVNext.Environment {
    public class OrchardContainerMiddleware {
        private readonly RequestDelegate _next;
        private readonly IShellSettingsManager _shellSettingsManager;
        private readonly IOrchardHost _orchardHost;
        private readonly IPackageAssemblyLookup _packageAssemblyLookup;
        private readonly IAssemblyLoadContextAccessor _assemblyLoadContextAccessor;


        public OrchardContainerMiddleware(
            RequestDelegate next,
            IShellSettingsManager shellSettingsManager,
            IOrchardHost orchardHost,
            IPackageAssemblyLookup packageAssemblyLookup,
            IAssemblyLoadContextAccessor assemblyLoadContextAccessor) {
            _next = next;
            _shellSettingsManager = shellSettingsManager;
            _orchardHost = orchardHost;
            _packageAssemblyLookup = packageAssemblyLookup;
            _assemblyLoadContextAccessor = assemblyLoadContextAccessor;

#if !(DNXCORE50)
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
#endif
        }

#if !(DNXCORE50)
        private Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args) {
            var assemblyName = new AssemblyName(args.Name);

            Logger.Debug("Resolving " + assemblyName);

            var packageLoader = _packageAssemblyLookup.GetLoaderForPackage(assemblyName.Name);

            if (packageLoader != null) {
                var lookup = packageLoader
                    .PackageAssemblyLookup
                    .Single(x => x.Key.Name == assemblyName.Name);

                var loader = new NuGetAssemblyLoader(
                    _assemblyLoadContextAccessor,
                    packageLoader);

                Logger.Debug("Resolved " + assemblyName);
                var assembly = loader.Load(lookup.Key);
                return assembly;
            }

            Logger.Debug("Cannot resolve " + assemblyName);
            return null;
        }
#endif

        public async Task Invoke(HttpContext httpContext) {
            var shellSetting = GetSettings(httpContext.Request.Host.Value);

            if (shellSetting != null) {
                using (var shell = _orchardHost.CreateShellContext(shellSetting)) {
                    httpContext.RequestServices = shell.LifetimeScope;

                    shell.Shell.Activate();
                    await _next.Invoke(httpContext);
                }
            }
            else {
                throw new Exception("Tenant not found");
            }
        }

        private ShellSettings GetSettings(string requestHost) {
            var shellSettings = _shellSettingsManager.LoadSettings();

            if (!shellSettings.Any()) {
                return new ShellSettings {Name = ShellSettings.DefaultName, State = TenantState.Uninitialized};
            }

            return shellSettings
                .SingleOrDefault(x => x.RequestUrlHost == requestHost);
        }
    }
}