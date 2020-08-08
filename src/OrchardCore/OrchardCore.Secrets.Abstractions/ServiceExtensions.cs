using Microsoft.Extensions.DependencyInjection;

namespace OrchardCore.Secrets
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddSecretLiquidFilter<T>(this IServiceCollection services, string name) where T : class, ISecretLiquidFilter
        {
            services.Configure<SecretOptions>(options => options.FilterRegistrations[name] = typeof(T));
            services.AddScoped<T>();
            return services;
        }
    }
}
