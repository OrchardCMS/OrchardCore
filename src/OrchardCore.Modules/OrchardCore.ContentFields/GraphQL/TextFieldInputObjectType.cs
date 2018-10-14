using OrchardCore.Apis.GraphQL.Queries;
using OrchardCore.ContentFields.Fields;

namespace OrchardCore.ContentFields.GraphQL
{
    public class TextFieldInputObjectType : QueryArgumentObjectGraphType<TextField>
    {
        public TextFieldInputObjectType()
        {
            Name = "TextFieldInput";

            AddInputField("Text", x => x.Text, true);
        }
    }
}
