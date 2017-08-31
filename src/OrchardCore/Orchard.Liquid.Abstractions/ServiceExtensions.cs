using Microsoft.Extensions.DependencyInjection;

namespace Orchard.Liquid
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddLiquidFilter<T>(this IServiceCollection services, string name) where T : class, ILiquidFilter
        {
            services.Configure<LiquidOptions>(options => options.FilterRegistrations.Add(name, typeof(T)));
            services.AddScoped<T>();
            return services;
        }
    }
}
