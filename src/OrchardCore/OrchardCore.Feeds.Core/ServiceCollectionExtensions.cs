using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Feeds.Rss;

namespace OrchardCore.Feeds
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddFeeds(this IServiceCollection services)
        {
            services.AddSingleton<IFeedBuilderProvider, RssFeedBuilderProvider>();

            return services;
        }
    }
}
