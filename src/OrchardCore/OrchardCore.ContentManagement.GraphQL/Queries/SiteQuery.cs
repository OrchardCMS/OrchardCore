using System.Linq;
using System.Threading.Tasks;
using GraphQL.Resolvers;
using GraphQL.Types;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using OrchardCore.Apis.GraphQL;
using OrchardCore.ContentManagement.GraphQL.Options;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.Settings;

namespace OrchardCore.ContentManagement.GraphQL.Queries
{
    /// <summary>
    /// Registers all Content Types as queries.
    /// </summary>
    public class SiteQuery : ISchemaBuilder
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IOptions<GraphQLContentOptions> _contentOptionsAccessor;
        private readonly ISiteService _siteService;
        private readonly IStringLocalizer S;

        public SiteQuery(IHttpContextAccessor httpContextAccessor,
            IOptions<GraphQLContentOptions> contentOptionsAccessor,
            ISiteService siteService,
            IStringLocalizer<SiteQuery> localizer)
        {
            _httpContextAccessor = httpContextAccessor;
            _contentOptionsAccessor = contentOptionsAccessor;
            _siteService = siteService;
            S = localizer;
        }

        public Task<IChangeToken> BuildAsync(ISchema schema)
        {
            var typetype = new SiteGraphType(_contentOptionsAccessor);
            var field = new FieldType {
                Name = "Site",
                Description = S["Site Metadata including Settings and CustomSettings."],
                ResolvedType = typetype,
                Resolver = new AsyncFieldResolver<ISite>(ResolveAsync)
            };

            field.RequirePermission(OrchardCore.Settings.Permissions.ManageSettings);

            var serviceProvider = _httpContextAccessor.HttpContext.RequestServices;
            var siteBuilders = serviceProvider.GetServices<ISiteGraphBuilder>().ToList();

            foreach (var siteBuilder in siteBuilders)
            {
                siteBuilder.Build(typetype);
            }

            schema.Query.AddField(field);

            var contentDefinitionManager = serviceProvider.GetService<IContentDefinitionManager>();

            return Task.FromResult(contentDefinitionManager.ChangeToken);
        }

        private Task<ISite> ResolveAsync(ResolveFieldContext context)
        {
            return _siteService.GetSiteSettingsAsync();
        }
    }
}
