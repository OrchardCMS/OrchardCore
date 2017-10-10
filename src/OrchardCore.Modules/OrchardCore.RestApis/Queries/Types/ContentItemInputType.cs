using GraphQL.Types;
using OrchardCore.ContentManagement.Metadata;

namespace OrchardCore.RestApis.Queries.Types
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
