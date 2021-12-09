using System.Reflection;
using OrchardCore.DisplayManagement.TagHelpers;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds tag helper services.
        /// </summary>
        public static IServiceCollection AddTagHelpers(this IServiceCollection services, Assembly assembly)
        {
            return services.AddTransient<ITagHelpersProvider>(sp => new AssemblyTagHelpersProvider(assembly));
        }

        /// <summary>
        /// Adds tag helper services.
        /// </summary>
        public static IServiceCollection AddTagHelpers<T>(this IServiceCollection services)
        {
            return services
                .AddTransient<ITagHelpersProvider>(sp => new TagHelpersProvider<T>())
                .AddTransient(typeof(T));
        }
    }
}
