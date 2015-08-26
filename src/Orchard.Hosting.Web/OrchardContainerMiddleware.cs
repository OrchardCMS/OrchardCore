using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Http;
using System;
using Microsoft.Dnx.Runtime;
using Orchard.Configuration.Environment;
using Orchard.DependencyInjection;
using Microsoft.Dnx.Compilation;
using Microsoft.Dnx.Compilation.Caching;
using Microsoft.Framework.Logging;

#if !(DNXCORE50)
using System.Reflection;
using Microsoft.Dnx.Runtime.Loader;
#endif

namespace Orchard.Hosting {
    public class OrchardContainerMiddleware {
        private readonly RequestDelegate _next;
        private readonly IShellSettingsManager _shellSettingsManager;
        private readonly IOrchardHost _orchardHost;
        private readonly IAssemblyLoadContextAccessor _assemblyLoadContextAccessor;
        private readonly IApplicationEnvironment _applicationEnvironement;
        private readonly IOrchardLibraryManager _libraryManager;
        private readonly ICache _cache;
        private readonly ILogger _logger;

        public OrchardContainerMiddleware(
            RequestDelegate next,
            IShellSettingsManager shellSettingsManager,
            IOrchardHost orchardHost,
            IAssemblyLoadContextAccessor assemblyLoadContextAccessor,
            IApplicationEnvironment applicationEnvironement,
            IOrchardLibraryManager libraryManager,
            ICache cache,
            ILoggerFactory loggerFactory) {
            _next = next;
            _shellSettingsManager = shellSettingsManager;
            _orchardHost = orchardHost;
            _assemblyLoadContextAccessor = assemblyLoadContextAccessor;
            _applicationEnvironement = applicationEnvironement;
            _libraryManager = libraryManager;
            _cache = cache;
            _logger = loggerFactory.CreateLogger<OrchardContainerMiddleware>();

#if !(DNXCORE50)
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
#endif
        }

#if !(DNXCORE50)
        private Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args) {
            return _cache.Get<Assembly>(args.Name, (context) => {
                var assemblyName = new AssemblyName(args.Name);

                _logger.LogDebug("Resolving {0}", assemblyName);

                var reference = _libraryManager.GetMetadataReference(assemblyName.Name);

                if (reference != null) {
                    if (reference is MetadataFileReference) {
                        var fileReference = (MetadataFileReference)reference;

                        var loadedAssembly = _assemblyLoadContextAccessor
                            .Default
                            .LoadFile(fileReference.Path);

                        if (loadedAssembly != null) {
                            _logger.LogDebug("Resolved {0}", assemblyName);
                            return loadedAssembly;
                        }
                    }
                }

                _logger.LogDebug("Cannot resolve {0}", assemblyName);
                return null;
            });
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