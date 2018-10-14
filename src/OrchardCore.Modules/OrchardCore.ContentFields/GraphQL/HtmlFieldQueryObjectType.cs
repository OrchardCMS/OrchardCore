using GraphQL.Types;
using OrchardCore.ContentFields.Fields;

namespace OrchardCore.ContentFields.GraphQL
{
    public class HtmlFieldQueryObjectType : ObjectGraphType<HtmlField>
    {
        public HtmlFieldQueryObjectType()
        {
            Name = "HtmlField";

            Field(x => x.Html);
        }
    }
}
