using GraphQL.Types;
using OrchardCore.ContentFields.Fields;

namespace OrchardCore.ContentFields.GraphQL
{
    public class BooleanFieldQueryObjectType : ObjectGraphType<BooleanField>
    {
        public BooleanFieldQueryObjectType()
        {
            Name = "BooleanField";

            Field(x => x.Value);
        }
    }
}
