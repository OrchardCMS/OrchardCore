using System.Threading.Tasks;
using GraphQL.Resolvers;
using Microsoft.Extensions.Localization;
using OrchardCore.Apis.GraphQL.Arguments;
using OrchardCore.Apis.GraphQL.Types;
using OrchardCore.Queries.Apis.GraphQL.Mutations.Types;

namespace OrchardCore.Queries.Apis.GraphQL.Mutations
{
    public class CreateQueryMutation<TSourceType> : MutationFieldType where TSourceType : Query, new()
    {
        public IStringLocalizer T { get; set; }

        public CreateQueryMutation(
            IQueryManager queryManager,
            IStringLocalizer<CreateQueryMutation<TSourceType>> t)
        {
            T = t;

            Name = "Create" + typeof(TSourceType).Name;

            Arguments = new AutoRegisteringQueryArguments<TSourceType>(new[] { "Source" });

            Type = typeof(CreateQueryOutcomeType<TSourceType>);

            Resolver = new SlowFuncFieldResolver<object, Task<object>>(async (context) => {
                var query = context.MapArgumentsTo<TSourceType>();

                await queryManager.SaveQueryAsync(query.Name , query);

                return query;
            });
        }
    }
}
