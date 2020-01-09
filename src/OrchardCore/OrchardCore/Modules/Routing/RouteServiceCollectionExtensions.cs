using OrchardCore.Abstractions.Routing;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class RouteServiceCollectionExtensions
    {
        /// <summary>
        /// Configures a friendly default area route for admin controllers.
        /// </summary>
        /// <param name="area">Module area, e.g. OrchardCore.Email</param>
        /// <param name="mappedArea">Email</param>
        /// <returns></returns>
        public static IServiceCollection ConfigureAdminAreaRouteMap(this IServiceCollection services, string area, string mappedArea)
        {
            services.Configure<ShellRouteOptions>(options => options.AdminAreaRouteMap[area] = mappedArea.Trim(' ', '/'));
            return services;
        }

        /// <summary>
        /// Configures a friendly default area route for controllers.
        /// </summary>
        /// <param name="area">Module area, e.g. OrchardCore.ContentLocalization</param>
        /// <param name="mappedArea">ContentLocalization</param>
        /// <returns></returns>
        public static IServiceCollection ConfigureAreaRouteMap(this IServiceCollection services, string area, string mappedArea)
        {
            services.Configure<ShellRouteOptions>(options => options.AreaRouteMap[area] = mappedArea.Trim(' ', '/'));
            return services;
        }
    }
}
