using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace OrchardCore.Modules
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds tag helper services.
        /// </summary>
        public static IServiceCollection AddTagHelpers(this IServiceCollection services, string assemblyName)
        {
            return services.AddTagHelpers(Assembly.Load(new AssemblyName(assemblyName)));
        }

        /// <summary>
        /// Adds tag helper services.
        /// </summary>
        public static IServiceCollection AddTagHelpers(this IServiceCollection services, Assembly assembly)
        {
            return services.AddSingleton<ITagHelpersProvider>(new TagHelpersProvider(assembly));
        }
    }
}
