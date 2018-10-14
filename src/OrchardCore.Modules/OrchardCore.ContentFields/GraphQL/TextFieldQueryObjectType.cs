using GraphQL.Types;
using OrchardCore.ContentFields.Fields;

namespace OrchardCore.ContentFields.GraphQL
{
    public class TextFieldQueryObjectType : ObjectGraphType<TextField>
    {
        public TextFieldQueryObjectType()
        {
            Name = "TextField";

            Field(x => x.Text, true);
        }
    }
}
