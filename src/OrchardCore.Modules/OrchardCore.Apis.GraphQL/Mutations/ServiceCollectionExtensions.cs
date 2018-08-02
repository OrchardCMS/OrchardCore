using Microsoft.Extensions.DependencyInjection;

namespace OrchardCore.Apis.GraphQL.Mutations
{
    public static class ServiceCollectionExtensions
    {
        public static void AddGraphQLMutations(this IServiceCollection services) {
            services.AddScoped<IApiUpdateModel, ApiUpdateModel>();
            services.AddTransient<MutationsSchema>();
        }
    }
}
