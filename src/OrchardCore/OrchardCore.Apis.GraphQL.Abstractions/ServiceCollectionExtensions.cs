using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Apis.GraphQL.Types;

namespace OrchardCore.Apis
{
    public static class ServiceCollectionExtensions
    {
        public static void AddGraphMutationType<T>(this IServiceCollection services) where T : MutationFieldType
        {
            services.AddScoped<T>();
            services.AddScoped<MutationFieldType, T>();
        }

        public static void AddGraphQueryType<T>(this IServiceCollection services) where T : QueryFieldType
        {
            services.AddScoped<T>();
            services.AddScoped<QueryFieldType, T>();
        }
    }
}
