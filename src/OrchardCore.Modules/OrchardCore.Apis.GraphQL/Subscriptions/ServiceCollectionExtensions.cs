using Microsoft.Extensions.DependencyInjection;

namespace OrchardCore.Apis.GraphQL.Subscriptions
{
    public static class ServiceCollectionExtensions
    {
        public static void AddGraphQLSubscriptions(this IServiceCollection services)
        {
            services.AddTransient<SubscriptionSchema>();
        }
    }
}
