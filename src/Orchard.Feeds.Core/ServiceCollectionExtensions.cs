using Microsoft.Extensions.DependencyInjection;
using Orchard.Feeds.Rss;

namespace Orchard.Feeds
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