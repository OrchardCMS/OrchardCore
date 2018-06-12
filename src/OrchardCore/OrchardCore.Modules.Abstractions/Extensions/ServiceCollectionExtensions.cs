using System.Reflection;
using OrchardCore.Modules;

namespace Microsoft.Extensions.DependencyInjection
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
