using GraphQL.Types;
using OrchardCore.ContentManagement;

namespace OrchardCore.Contents.Apis.GraphQL.Mutations.Types
{
    public class ContentItemInputType : InputObjectGraphType<ContentItem>
    {
        public ContentItemInputType()
        {
            Name = "ContentItemInput";

            Field<StringGraphType>("ContentType");
            Field<StringGraphType>("Owner");
            Field<StringGraphType>("Author");

            Field<BooleanGraphType>("Published");

            Field<StringGraphType>("ContentParts");
        }
    }
}
