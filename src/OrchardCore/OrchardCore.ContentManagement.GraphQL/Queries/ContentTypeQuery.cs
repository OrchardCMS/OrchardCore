using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphQL.Types;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using OrchardCore.Apis.GraphQL;
using OrchardCore.ContentManagement.GraphQL.Options;
using OrchardCore.ContentManagement.GraphQL.Queries.Types;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;

namespace OrchardCore.ContentManagement.GraphQL.Queries
{
    /// <summary>
    /// Registers all Content Types as queries.
    /// </summary>
    public class ContentTypeQuery : ISchemaBuilder
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IOptions<GraphQLContentOptions> _optionsAccessor;

        public ContentTypeQuery(IHttpContextAccessor httpContextAccessor,
            IOptions<GraphQLContentOptions> optionsAccessor,
            IStringLocalizer<ContentTypeQuery> localizer)
        {
            _httpContextAccessor = httpContextAccessor;
            _optionsAccessor = optionsAccessor;
            T = localizer;
        }

        public IStringLocalizer T { get; set; }

        public Task<IChangeToken> BuildAsync(ISchema schema)
        {
            var serviceProvider = _httpContextAccessor.HttpContext.RequestServices;

            var contentDefinitionManager = serviceProvider.GetService<IContentDefinitionManager>();
            var contentTypeBuilders = serviceProvider.GetServices<IContentTypeBuilder>().ToList();

            foreach (var typeDefinition in contentDefinitionManager.ListTypeDefinitions())
            {
                var typeType = new ContentItemType(_optionsAccessor)
                {
                    Name = typeDefinition.Name,
                    Description = T["Represents a {0}.", typeDefinition.DisplayName]
                };

                var query = new ContentItemsFieldType(typeDefinition.Name, schema)
                {
                    Name = typeDefinition.Name,
                    Description = T["Represents a {0}.", typeDefinition.DisplayName],
                    ResolvedType = new ListGraphType(typeType)
                };

                foreach (var builder in contentTypeBuilders)
                {
                    builder.Build(query, typeDefinition, typeType);
                }

                var settings = typeDefinition.Settings?.ToObject<ContentTypeSettings>();

                // Only add queries over standard content types
                if (settings == null || String.IsNullOrWhiteSpace(settings.Stereotype))
                {
                    schema.Query.AddField(query);
                }
                else
                {
                    // Register the content item type explicitly since it won't be discovered from the root 'query' type.
                    schema.RegisterType(typeType);
                }
            }

            return Task.FromResult(contentDefinitionManager.ChangeToken);
        }
    }
}
