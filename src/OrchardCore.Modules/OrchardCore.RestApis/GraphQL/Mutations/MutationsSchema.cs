using System.Collections.Generic;
using GraphQL.Types;

namespace OrchardCore.RestApis.GraphQL.Mutations
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
