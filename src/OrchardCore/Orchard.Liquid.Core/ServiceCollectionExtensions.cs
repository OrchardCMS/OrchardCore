using Orchard.Liquid;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddLiquid(this IServiceCollection services)
        {
            services.AddSingleton<ILiquidManager, LiquidManager>();

            return services;
        }
    }
}