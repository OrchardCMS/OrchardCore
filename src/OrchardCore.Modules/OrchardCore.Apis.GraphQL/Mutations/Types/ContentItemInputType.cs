using GraphQL.Types;
using OrchardCore.ContentManagement.Metadata;

namespace OrchardCore.Apis.GraphQL.Mutations.Types
{
    public class ContentItemInputType : InputObjectGraphType
    {
        public ContentItemInputType(IContentDefinitionManager contentDefinitionManager)
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
