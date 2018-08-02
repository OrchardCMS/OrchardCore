using GraphQL.Types;

namespace OrchardCore.Queries.GraphQL.Mutations.Types
{
    public class CreateQueryOutcomeType<TSourceType> : AutoRegisteringObjectGraphType<TSourceType>
    {
        public CreateQueryOutcomeType()
        {
            Name = typeof(TSourceType).Name;
        }
    }
}
