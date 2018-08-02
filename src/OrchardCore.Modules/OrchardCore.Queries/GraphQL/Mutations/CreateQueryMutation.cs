using GraphQL.Resolvers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Apis.GraphQL.Arguments;
using OrchardCore.Apis.GraphQL.Types;
using OrchardCore.Queries.GraphQL.Mutations.Types;

namespace OrchardCore.Queries.GraphQL.Mutations
{
    public class CreateQueryMutation<TSourceType> : MutationFieldType where TSourceType : Query, new()
    {
        public CreateQueryMutation(IHttpContextAccessor httpContextAccessor)
        {

            Name = "Create" + typeof(TSourceType).Name;

            Arguments = new AutoRegisteringQueryArguments<TSourceType>(
                new[] { "Name" },
                new[] { "Source" }
            );

            Type = typeof(CreateQueryOutcomeType<TSourceType>);

            Resolver = new AsyncFieldResolver<object, object>(async (context) =>
            {
                var query = context.MapArgumentsTo<TSourceType>();

                var queryManager = httpContextAccessor.HttpContext.RequestServices.GetRequiredService<IQueryManager>();
                await queryManager.SaveQueryAsync(query.Name, query);

                return query;
            });
        }
    }
}
