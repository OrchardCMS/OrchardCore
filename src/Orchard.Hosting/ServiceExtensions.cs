using Microsoft.Dnx.Runtime;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.DependencyInjection.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

#if DNXCORE50
#endif

namespace Orchard.Hosting {
    public static class ServiceExtensions
    {
        internal static IServiceCollection AddFallback([NotNull] this IServiceCollection services) {

            services.AddInstance<IRuntimeServices>(new ServiceManifest(services));

            return services;
        }
        
        private class ServiceManifest : IRuntimeServices {
            public ServiceManifest(IServiceCollection fallback) {

                var manifestTypes = fallback.Where(t => t.ServiceType.GetTypeInfo().GenericTypeParameters.Length == 0
                        && t.ServiceType != typeof(IRuntimeServices)
                        && t.ServiceType != typeof(IServiceProvider))
                        .Select(t => t.ServiceType)
                        .Distinct();

                Services = manifestTypes;
            }

            public IEnumerable<Type> Services { get; private set; }
        }
    }
}
