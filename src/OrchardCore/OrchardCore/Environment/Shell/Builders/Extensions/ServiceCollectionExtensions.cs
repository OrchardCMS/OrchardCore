using System;
using Microsoft.Extensions.DependencyInjection;

namespace OrchardCore.Environment.Shell.Builders
{
    internal static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddHostSingleton(
            this IServiceCollection services,
            ServiceDescriptor hostDescriptor,
            object implementationInstance)
        {
            var serviceDescriptor = new HostSingleton(hostDescriptor, implementationInstance);
            services.Add(serviceDescriptor);
            return services;
        }

        public static IServiceCollection AddHostSingleton(
            this IServiceCollection collection,
            ServiceDescriptor hostDescriptor,
            Func<IServiceProvider, object> implementationFactory)
        {
            var descriptor = new HostSingleton(hostDescriptor, implementationFactory);
            collection.Add(descriptor);
            return collection;
        }
    }
}