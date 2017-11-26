using System.Collections.Generic;
using GraphQL.Types;
using OrchardCore.Apis.GraphQL.Types;

namespace OrchardCore.Apis.GraphQL.Mutations
{
    public class MutationsSchema : ObjectGraphType
    {
        public MutationsSchema(IEnumerable<MutationFieldType> mutationFields)
        {
            Name = "Mutations";

            foreach (var mutationField in mutationFields)
            {
                AddField(mutationField);
            }
        }
    }
}
