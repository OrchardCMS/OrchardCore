using GraphQL.Types;
using OrchardCore.ContentFields.Fields;

namespace OrchardCore.ContentFields.GraphQL
{
    public class NumericFieldQueryObjectType : ObjectGraphType<NumericField>
    {
        public NumericFieldQueryObjectType()
        {
            Name = "NumericField";

            Field(x => x.Value.Value);
        }
    }
}
