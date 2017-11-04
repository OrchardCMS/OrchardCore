using GraphQL.Types;

namespace OrchardCore.Apis.GraphQL.Mutations.Types
{
    public class ContentItemInputType : InputObjectGraphType
    {
        public ContentItemInputType()
        {
            Name = "ContentItemInput";

            Field<StringGraphType>("contentType");
            Field<StringGraphType>("owner");
            Field<StringGraphType>("author");

            Field<BooleanGraphType>("published");

            Field<StringGraphType>("contentParts");
        }
    }
}
