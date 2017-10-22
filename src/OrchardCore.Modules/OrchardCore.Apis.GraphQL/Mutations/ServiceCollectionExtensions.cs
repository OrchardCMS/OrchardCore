using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Apis.GraphQL.Mutations.Types;

namespace OrchardCore.Apis.GraphQL.Mutations
{
    public static class ServiceCollectionExtensions
    {
        public static void AddGraphQLMutations(this IServiceCollection services) {
            services.AddScoped<MutationsSchema>();
            services.AddGraphMutationType<CreateContentItemMutation>();
            services.AddScoped<ContentItemInputType>();
        }

        public static void AddGraphMutationType<T>(this IServiceCollection services) where T : MutationFieldType
        {
            services.AddScoped<T>();
            services.AddScoped<MutationFieldType, T>();
        }
    }
}
