using System.Linq;
using GraphQL.Types;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.ContentManagement.GraphQL.Options;
using OrchardCore.ContentManagement.Metadata.Models;

namespace OrchardCore.ContentManagement.GraphQL.Queries.Types
{
    public class DynamicContentTypeInputBuilder : DynamicContentTypeBuilder
    {
        public DynamicContentTypeInputBuilder(IHttpContextAccessor httpContextAccessor,
            IOptions<GraphQLContentOptions> contentOptionsAccessor,
            IStringLocalizer<DynamicContentTypeInputBuilder> localizer)
            : base(httpContextAccessor, contentOptionsAccessor, localizer) { }

        public override void Build(ISchema schema, FieldType contentQuery, ContentTypeDefinition contentTypeDefinition, ContentItemType contentItemType)
        {
            var whereInputType = (ContentItemWhereInput)contentQuery.Arguments?.FirstOrDefault(x => x.Name == "where")?.ResolvedType;

            if (whereInputType != null)
            {
                BuildInternal(schema, contentQuery, contentTypeDefinition, whereInputType);
            }
        }
    }
}
