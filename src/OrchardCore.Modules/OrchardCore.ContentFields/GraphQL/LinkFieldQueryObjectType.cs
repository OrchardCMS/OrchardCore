using GraphQL.Types;
using OrchardCore.ContentFields.Fields;

namespace OrchardCore.ContentFields.GraphQL
{
    public class LinkFieldQueryObjectType : ObjectGraphType<LinkField>
    {
        public LinkFieldQueryObjectType()
        {
            Name = "LinkField";

            Field(x => x.Text);
            Field(x => x.Url);
        }
    }
}
