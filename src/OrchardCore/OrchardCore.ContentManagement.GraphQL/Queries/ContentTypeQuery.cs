using System.Linq;
using System.Threading.Tasks;
using GraphQL.Types;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.Apis.GraphQL;
using OrchardCore.ContentManagement.GraphQL.Options;
using OrchardCore.ContentManagement.GraphQL.Queries.Types;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;

namespace OrchardCore.ContentManagement.GraphQL.Queries
{
    /// <summary>
    /// Registers all Content Types as queries.
    /// </summary>
    public class ContentTypeQuery : ISchemaBuilder
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IOptions<GraphQLContentOptions> _contentOptionsAccessor;
        private readonly IOptions<GraphQLSettings> _settingsAccessor;
        protected readonly IStringLocalizer S;

        public ContentTypeQuery(IHttpContextAccessor httpContextAccessor,
            IOptions<GraphQLContentOptions> contentOptionsAccessor,
            IOptions<GraphQLSettings> settingsAccessor,
            IStringLocalizer<ContentTypeQuery> localizer)
        {
            _httpContextAccessor = httpContextAccessor;
            _contentOptionsAccessor = contentOptionsAccessor;
            _settingsAccessor = settingsAccessor;
            S = localizer;
        }

        public Task<string> GetIdentifierAsync()
        {
            var contentDefinitionManager = _httpContextAccessor.HttpContext.RequestServices.GetService<IContentDefinitionManager>();
            return contentDefinitionManager.GetIdentifierAsync();
        }

        public Task BuildAsync(ISchema schema)
        {
            var serviceProvider = _httpContextAccessor.HttpContext.RequestServices;

            var contentDefinitionManager = serviceProvider.GetService<IContentDefinitionManager>();
            var contentTypeBuilders = serviceProvider.GetServices<IContentTypeBuilder>().ToList();

            foreach (var typeDefinition in contentDefinitionManager.ListTypeDefinitions())
            {
                if (_contentOptionsAccessor.Value.ShouldHide(typeDefinition))
                {
                    continue;
                }

                var typeType = new ContentItemType(_contentOptionsAccessor)
                {
                    Name = typeDefinition.Name,
                    Description = S["Represents a {0}.", typeDefinition.DisplayName]
                };

                var query = new ContentItemsFieldType(typeDefinition.Name, schema, _contentOptionsAccessor, _settingsAccessor)
                {
                    Name = typeDefinition.Name,
                    Description = S["Represents a {0}.", typeDefinition.DisplayName],
                    ResolvedType = new ListGraphType(typeType)
                };

                query.RequirePermission(CommonPermissions.ExecuteGraphQL);
                query.RequirePermission(Contents.CommonPermissions.ViewOwnContent, typeDefinition.Name);

                foreach (var builder in contentTypeBuilders)
                {
                    builder.Build(query, typeDefinition, typeType);
                }

                // Only add queries over standard content types
                if (!typeDefinition.HasStereotype())
                {
                    schema.Query.AddField(query);
                }
                else
                {
                    // Register the content item type explicitly since it won't be discovered from the root 'query' type.
                    schema.RegisterType(typeType);
                }
            }

            foreach (var builder in contentTypeBuilders)
            {
                builder.Clear();
            }

            return Task.CompletedTask;
        }
    }
}
