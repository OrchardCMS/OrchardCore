using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.PlatformAbstractions;
using System;

namespace Orchard.Environment.Shell.Builders
{
    public class FallbackServiceProvider : IServiceProvider
    {
        private readonly IServiceProvider _services;

        public FallbackServiceProvider(
            IServiceProvider fallback,
            IServiceCollection replacedServices)
        {
            var services = new ServiceCollection();
            var manifest = fallback.GetRequiredService<IRuntimeServices>();
            foreach (var service in manifest.Services)
            {
                services.AddTransient(service, sp => fallback.GetService(service));
            }

            services.Add(replacedServices);

            _services = services.BuildServiceProvider();
        }

        public object GetService(Type serviceType)
        {
            return _services.GetService(serviceType);
        }
    }
}
