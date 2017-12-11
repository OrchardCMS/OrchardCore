using GraphQL.Types;

namespace OrchardCore.Queries.Apis.GraphQL.Mutations.Types
{
    public class CreateQueryOutcomeType<TSourceType> : AutoRegisteringObjectGraphType<TSourceType>
    {
        public CreateQueryOutcomeType()
        {
            Name = typeof(TSourceType).Name;
        }
    }
}
