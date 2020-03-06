using System.Linq;
using GraphQL.Resolvers;
using GraphQL.Types;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.Apis.GraphQL;
using OrchardCore.ContentManagement.GraphQL.Options;
using OrchardCore.ContentManagement.GraphQL.Queries.Types;
using OrchardCore.Contents;
using OrchardCore.CustomSettings.Services;

namespace OrchardCore.ContentManagement.GraphQL.Queries
{
    public sealed class CustomSettingsSiteGraphBuilder : ISiteGraphBuilder
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IOptions<GraphQLContentOptions> _contentOptionsAccessor;
        private readonly CustomSettingsService _customSettings;
        private readonly IStringLocalizer S;

        public CustomSettingsSiteGraphBuilder(
            IOptions<GraphQLContentOptions> contentOptionsAccessor,
            IHttpContextAccessor httpContextAccessor,
            CustomSettingsService customSettings,
            IStringLocalizer<DynamicContentTypeBuilder> localizer
        ) {
            _httpContextAccessor = httpContextAccessor;
            _contentOptionsAccessor = contentOptionsAccessor;
            _customSettings = customSettings;

            S = localizer;
        }

        public void Build(SiteGraphType siteType)
        {
            var serviceProvider = _httpContextAccessor.HttpContext.RequestServices;
            var contentTypeBuilders = serviceProvider.GetServices<IContentTypeBuilder>().ToList();
            var settingsTypes = _customSettings.GetAllSettingsTypes();

            foreach (var typeDefinition in settingsTypes)
            {
                var typeType = new ContentItemType(_contentOptionsAccessor)
                {
                    ContentType = typeDefinition.Name,
                    Name = $"Site__{typeDefinition.Name}",
                    Description = S["Represents a {0}.", typeDefinition.DisplayName]
                };

                var query = new FieldType
                {
                    Name = typeDefinition.Name,
                    Description = S["Represents a {0}.", typeDefinition.DisplayName],
                    ResolvedType = typeType,
                    Resolver = new AsyncFieldResolver<ContentItem>(async context =>
                    {
                        var customSetting = await _customSettings.GetSettingsAsync(typeDefinition.Name);

                        return customSetting;
                    })
                };

                query.RequirePermission(CommonPermissions.ViewContent, typeDefinition.Name);

                foreach (var builder in contentTypeBuilders)
                {
                    builder.Build(query, typeDefinition, typeType);
                }

                siteType.AddField(query);
            }

            foreach (var builder in contentTypeBuilders)
            {
                builder.Clear();
            }
        }
    }
}
