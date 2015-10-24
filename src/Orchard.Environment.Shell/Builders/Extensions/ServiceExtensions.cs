using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.PlatformAbstractions;
using System;

namespace Orchard.Environment.Shell.Builders {
    public static class ServiceExtensions
    {
        public static IServiceProvider BuildShellServiceProviderWithHost(
            this IServiceCollection services,
            IServiceProvider hostServices) {

            return new WrappingServiceProvider(hostServices, services);
        }

        private class WrappingServiceProvider : IServiceProvider {
            private readonly IServiceProvider _services;

            // Need full wrap for generics like IOptions
            public WrappingServiceProvider(IServiceProvider fallback, IServiceCollection replacedServices) {
                var services = new ServiceCollection();
                var manifest = fallback.GetRequiredService<IRuntimeServices>();
                foreach (var service in manifest.Services) {
                    services.AddTransient(service, sp => fallback.GetService(service));
                }
                
                services.Add(replacedServices);

                _services = services.BuildServiceProvider();
            }

            public object GetService(Type serviceType) {
                return _services.GetService(serviceType);
            }
        }
    }
}
