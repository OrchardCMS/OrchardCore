using GraphQL.Types;
using OrchardCore.ContentFields.Fields;

namespace OrchardCore.ContentFields.GraphQL.Types
{
    public class LinkFieldQueryObjectType : ObjectGraphType<LinkField>
    {
        public LinkFieldQueryObjectType()
        {
            Name = nameof(LinkField);

            // HACK: Queries fail unless ResolvedType (Type(new StringGraphType()) is set and
            // name is specified lowercase. Very confusing since this hack isn't necessary anywhere else.
            Field("url", x => x.Url, nullable: true).Type(new StringGraphType());
            Field("text", x => x.Text, nullable: true).Type(new StringGraphType());
        }
    }
}