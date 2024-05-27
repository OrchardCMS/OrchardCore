using System.Collections.Generic;
using GraphQL.Types;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.ContentManagement.GraphQL.Options;
using OrchardCore.ContentManagement.Metadata.Models;

namespace OrchardCore.ContentManagement.GraphQL.Queries.Types
{
    public class DynamicContentTypeQueryBuilder : DynamicContentTypeBuilder
    {
        public DynamicContentTypeQueryBuilder(IHttpContextAccessor httpContextAccessor,
            IEnumerable<IContentFieldProvider> contentFieldProviders,
            IOptions<GraphQLContentOptions> contentOptionsAccessor,
            IStringLocalizer<DynamicContentTypeQueryBuilder> localizer)
            : base(httpContextAccessor, contentFieldProviders, contentOptionsAccessor, localizer) { }

        public override void Build(FieldType contentQuery, ContentTypeDefinition contentTypeDefinition, ContentItemType contentItemType)
        {
            BuildInternal(contentQuery, contentTypeDefinition, contentItemType);
        }
    }
}