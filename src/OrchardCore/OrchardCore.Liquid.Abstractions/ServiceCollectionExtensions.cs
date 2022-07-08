using Fluid;
using Microsoft.Extensions.DependencyInjection;

namespace OrchardCore.Liquid
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddLiquidFilter<TLiquidFilter>(this IServiceCollection services, string name) where TLiquidFilter : class, ILiquidFilter
        {
            services.Configure<TemplateOptions>(o => o.Filters.AddFilter(name, new LiquidFilterDelegateResolver<TLiquidFilter>().ResolveAsync));
            services.AddScoped<TLiquidFilter>();

            return services;
        }
    }
}
