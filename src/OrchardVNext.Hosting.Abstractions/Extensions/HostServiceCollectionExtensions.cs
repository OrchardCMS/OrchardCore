using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Dnx.Compilation.Caching;
using Microsoft.Dnx.Runtime;
using Microsoft.Framework.DependencyInjection;
using OrchardVNext.FileSystem;

namespace OrchardVNext.Hosting {
    public static class WebServiceCollectionExtensions {
        public static IServiceCollection AddHost(
            [NotNull] this IServiceCollection services, Action<IServiceCollection> additionalDependencies) {

            services.AddFileSystems();

            // Caching - Move out
            services.AddInstance<ICacheContextAccessor>(new CacheContextAccessor());
            services.AddSingleton<ICache, Cache>();

            additionalDependencies(services);

            services.AddTransient<IShellHost, ShellHost>();

            return services.AddFallback();
        }

        internal static IServiceCollection AddFallback([NotNull] this IServiceCollection services) {

            services.AddInstance<IRuntimeServices>(new ServiceManifest(services));

            return services;
        }

        internal class ServiceManifest : IRuntimeServices {
            public ServiceManifest(IServiceCollection fallback) {

                var manifestTypes = fallback.Where(t => t.ServiceType.GetTypeInfo().GenericTypeParameters.Length == 0
                        && t.ServiceType != typeof(IRuntimeServices)
                        && t.ServiceType != typeof(IServiceProvider))
                        .Select(t => t.ServiceType).Distinct();

                Services = manifestTypes;
            }

            public IEnumerable<Type> Services { get; private set; }
        }
    }
}