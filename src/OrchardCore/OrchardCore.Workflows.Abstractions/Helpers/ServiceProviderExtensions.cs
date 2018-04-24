using System;
using System.Linq;

namespace OrchardCore.Workflows.Helpers
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
            var constructor = type.GetConstructors()[0];

            if (constructor != null)
            {
                var args = constructor
                    .GetParameters()
                    .Select(o => o.ParameterType)
                    .Select(o => provider.GetService(o))
                    .ToArray();

                return Activator.CreateInstance(type, args) as TResult;
            }

            return null;
        }
    }
}
