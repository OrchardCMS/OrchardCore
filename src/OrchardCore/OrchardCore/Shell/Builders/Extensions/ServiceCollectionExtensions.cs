using System;
using Microsoft.Extensions.DependencyInjection;

namespace OrchardCore.Environment.Shell.Builders
{
    internal static class ServiceCollectionExtensions
    {
        public static IServiceCollection CloneSingleton(
            this IServiceCollection services,
            ServiceDescriptor parent,
            object implementationInstance)
        {
            var cloned = new ClonedSingletonDescriptor(parent, implementationInstance);
            services.Add(cloned);
            return services;
        }

        public static IServiceCollection CloneSingleton(
            this IServiceCollection collection,
            ServiceDescriptor parent,
            Func<IServiceProvider, object> implementationFactory)
        {
            var cloned = new ClonedSingletonDescriptor(parent, implementationFactory);
            collection.Add(cloned);
            return collection;
        }
    }
}
