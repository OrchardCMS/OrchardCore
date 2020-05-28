using GraphQL.Types;
using OrchardCore.ContentFields.Fields;

namespace OrchardCore.ContentFields.GraphQL.Types
{
    public class LinkFieldQueryObjectType : ObjectGraphType<LinkField>
    {
        public LinkFieldQueryObjectType()
        {
            Name = nameof(LinkField);

            Field(x => x.Url, nullable: true).Description("the url of the link");
            Field(x => x.Text, nullable: true).Description("the text of the link");
        }
    }
}
