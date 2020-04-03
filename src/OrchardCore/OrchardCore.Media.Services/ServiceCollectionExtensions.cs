using Microsoft.Extensions.DependencyInjection;

namespace OrchardCore.Media
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMedia(this IServiceCollection services)
        {
            services.AddSingleton<IMediaService, MediaService>();

            services.AddScoped<IMediaFactorySelector, ImageFactorySelector>();

            return services;
        }
    }
}