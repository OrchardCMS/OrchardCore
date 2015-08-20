using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Http;
using System;
using Microsoft.Dnx.Runtime;
using OrchardVNext.Configuration.Environment;
using OrchardVNext.Hosting.Extensions;
using System.Collections.Generic;
using NuGet;

#if !(DNXCORE50)
using System.Reflection;
using Microsoft.Dnx.Runtime.Loader;
#endif

namespace OrchardVNext.Hosting {
    public class OrchardContainerMiddleware {
        private readonly RequestDelegate _next;
        private readonly IShellSettingsManager _shellSettingsManager;
        private readonly IOrchardHost _orchardHost;
        private readonly IPackageAssemblyLookup _packageAssemblyLookup;
        private readonly IAssemblyLoadContextAccessor _assemblyLoadContextAccessor;
        private readonly IApplicationEnvironment _applicationEnvironement;
        private readonly ILibraryManager _libraryManager;

        public OrchardContainerMiddleware(
            RequestDelegate next,
            IShellSettingsManager shellSettingsManager,
            IOrchardHost orchardHost,
            IPackageAssemblyLookup packageAssemblyLookup,
            IAssemblyLoadContextAccessor assemblyLoadContextAccessor,
            IApplicationEnvironment applicationEnvironement,
            ILibraryManager libraryManager) {
            _next = next;
            _shellSettingsManager = shellSettingsManager;
            _orchardHost = orchardHost;
            _packageAssemblyLookup = packageAssemblyLookup;
            _assemblyLoadContextAccessor = assemblyLoadContextAccessor;
            _applicationEnvironement = applicationEnvironement;
            _libraryManager = libraryManager;

#if !(DNXCORE50)
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
#endif
        }

#if !(DNXCORE50)
        private Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args) {
            var assemblyName = new AssemblyName(args.Name);

            Logger.Debug("Resolving " + assemblyName);
            var context = _assemblyLoadContextAccessor.Default;

            var repository = new PackageRepository(_packageAssemblyLookup.PackagePaths.First());

            var packages = repository.FindPackagesById(assemblyName.Name);

            if (packages.Any()) {
                var latestPackage = packages.Last(); //IsLAtestVersion and IsAbsoluteLAtest not working.
                


                var assembly = context.Load(new AssemblyName(assemblyName.Name));
                if (assembly != null) {
                    Logger.Debug("Resolved " + assemblyName);
                    return assembly;
                }
            }

            //IDictionary<string, IEnumerable<NuGet.PackageInfo>> repository = _packageAssemblyLookup
            //    .Repositories
            //    .SelectMany(x => x.GetAllPackages())
            //    .ToDictionary(x => x.Key, y => y.Value);

            //if (repository.ContainsKey(assemblyName.Name)) {
            //    var packageInfo = repository[assemblyName.Name].Last();


            //    var assembly = Assembly.Load(new AssemblyName(assemblyName.Name));
            //    return assembly;
            //}

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