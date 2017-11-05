using GraphQL.Types;

namespace OrchardCore.Contents.Apis.GraphQL.Mutations.Types
{
    public class ContentItemInputType : InputObjectGraphType
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
