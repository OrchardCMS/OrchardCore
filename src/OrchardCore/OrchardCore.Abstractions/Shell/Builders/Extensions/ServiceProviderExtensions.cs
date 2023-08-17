using System;
using Microsoft.Extensions.DependencyInjection;

namespace OrchardCore.Environment.Shell.Builders
{
    public static class ServiceProviderExtensions
    {
        /// <summary>
        /// Instantiates a new object of the specified type, but with support for constructor injection.
        /// </summary>
        public static TResult CreateInstance<TResult>(this IServiceProvider provider) where TResult : class
        {
            return CreateInstance<TResult>(provider, typeof(TResult));
        }

        /// <summary>
        /// Instantiates a new object of the specified type, but with support for constructor injection.
        /// </summary>
        public static TResult CreateInstance<TResult>(this IServiceProvider provider, Type type) where TResult : class
        {
            return (TResult)ActivatorUtilities.CreateInstance(provider, type);
        }
    }
}
