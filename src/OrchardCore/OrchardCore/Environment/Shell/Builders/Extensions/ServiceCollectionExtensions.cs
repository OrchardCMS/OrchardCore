using System;
using Microsoft.Extensions.DependencyInjection;

namespace OrchardCore.Environment.Shell.Builders
{
    internal static class ServiceCollectionExtensions
    {
        public static IServiceCollection CloneSingleton(
            this IServiceCollection services,
            ServiceDescriptor hostDescriptor,
            object implementationInstance)
        {
            var descriptor = new ClonedSingletonDescriptor(hostDescriptor, implementationInstance);
            services.Add(descriptor);
            return services;
        }

        public static IServiceCollection CloneSingleton(
            this IServiceCollection collection,
            ServiceDescriptor hostDescriptor,
            Func<IServiceProvider, object> implementationFactory)
        {
            var descriptor = new ClonedSingletonDescriptor(hostDescriptor, implementationFactory);
            collection.Add(descriptor);
            return collection;
        }
    }
}