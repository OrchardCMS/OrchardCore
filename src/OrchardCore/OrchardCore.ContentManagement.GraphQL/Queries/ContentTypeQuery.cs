using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphQL.Types;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Apis.GraphQL;
using OrchardCore.ContentManagement.GraphQL.Queries.Types;
using OrchardCore.ContentManagement.Metadata;

namespace OrchardCore.ContentManagement.GraphQL.Queries
{
    /// <summary>
    /// Registers all Content Types as queries.
    /// </summary>
    public class ContentTypeQuery : ISchemaBuilder
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ContentTypeQuery(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public Task<IChangeToken> BuildAsync(ISchema schema)
        {
            var serviceProvider = _httpContextAccessor.HttpContext.RequestServices;

            var contentDefinitionManager = serviceProvider.GetService<IContentDefinitionManager>();
            var contentTypeBuilders = serviceProvider.GetService<IEnumerable<IContentTypeBuilder>>().ToList();

            foreach (var typeDefinition in contentDefinitionManager.ListTypeDefinitions())
            {
                var typeType = new ContentItemType
                {
                    Name = typeDefinition.Name
                };

                var query = new ContentItemsFieldType(_httpContextAccessor)
                {
                    Name = typeDefinition.Name,
                    ResolvedType = new ListGraphType(typeType)
                };

                foreach (var builder in contentTypeBuilders)
                {
                    builder.Build(query, typeDefinition, typeType);
                }

                schema.Query.AddField(query);
            }

            return Task.FromResult(contentDefinitionManager.ChangeToken);
        }
    }
}
