using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using OrchardCore.AdminMenu.Services;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.Contents.Security;
using OrchardCore.Navigation;

namespace OrchardCore.Contents.AdminNodes
{
    public class ContentTypesAdminNodeNavigationBuilder : IAdminNodeNavigationBuilder
    {
        private readonly LinkGenerator _linkGenerator;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IAuthorizationService _authorizationService;
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly ILogger _logger;

        public ContentTypesAdminNodeNavigationBuilder(
            IContentDefinitionManager contentDefinitionManager,
            LinkGenerator linkGenerator,
            IHttpContextAccessor httpContextAccessor,
            IAuthorizationService authorizationService,
            ILogger<ContentTypesAdminNodeNavigationBuilder> logger)
        {
            _contentDefinitionManager = contentDefinitionManager;
            _linkGenerator = linkGenerator;
            _httpContextAccessor = httpContextAccessor;
            _authorizationService = authorizationService;
            _logger = logger;
        }

        public string Name => typeof(ContentTypesAdminNode).Name;

        public async Task BuildNavigationAsync(MenuItem menuItem, NavigationBuilder builder, IEnumerable<IAdminNodeNavigationBuilder> treeNodeBuilders)
        {
            var node = menuItem as ContentTypesAdminNode;

            if (node == null || !node.Enabled)
            {
                return;
            }

            // Add ContentTypes specific children
            var typesToShow = await GetListableContentTypeDefinitionsAsync(node);
            foreach (var ctd in typesToShow)
            {
                builder.Add(new LocalizedString(ctd.DisplayName, ctd.DisplayName), cTypeMenu =>
                {
                    cTypeMenu.Url(_linkGenerator.GetPathByRouteValues(_httpContextAccessor.HttpContext, String.Empty, new
                    {
                        area = "OrchardCore.Contents",
                        controller = "Admin",
                        action = "List",
                        contentTypeId = ctd.Name
                    }));

                    cTypeMenu.Priority(node.Priority);
                    cTypeMenu.Position(node.Position);
                    cTypeMenu.Permission(
                        ContentTypePermissionsHelper.CreateDynamicPermission(ContentTypePermissionsHelper.PermissionTemplates[CommonPermissions.ViewContent.Name], ctd));

                    GetIconClasses(ctd, node).ToList().ForEach(c => cTypeMenu.AddClass(c));
                });
            }

            // Add external children
            foreach (var childNode in node.Items)
            {
                try
                {
                    var treeBuilder = treeNodeBuilders.Where(x => x.Name == childNode.GetType().Name).FirstOrDefault();
                    await treeBuilder.BuildNavigationAsync(childNode, builder, treeNodeBuilders);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "An exception occurred while building the '{MenuItem}' child Menu Item.", childNode.GetType().Name);
                }
            }
        }

        private async Task<IEnumerable<ContentTypeDefinition>> GetListableContentTypeDefinitionsAsync(ContentTypesAdminNode node)
        {
            var contentTypeDefinitions = _contentDefinitionManager.ListTypeDefinitions();

            var listableContentTypeDefinitions = new List<ContentTypeDefinition>();

            foreach (var contentTypeDefinition in contentTypeDefinitions)
            {
                if (!node.ShowAll && !node.ContentTypes.Any(entry => String.Equals(contentTypeDefinition.Name, entry.ContentTypeId, StringComparison.OrdinalIgnoreCase)))
                {
                    continue;
                }

                if (!await _authorizationService.AuthorizeContentTypeAsync(_httpContextAccessor.HttpContext.User, CommonPermissions.ListContent, contentTypeDefinition))
                {
                    continue;
                }

                listableContentTypeDefinitions.Add(contentTypeDefinition);
            }

            return listableContentTypeDefinitions.OrderBy(t => t.DisplayName);
        }

        private static List<string> GetIconClasses(ContentTypeDefinition contentType, ContentTypesAdminNode node)
        {
            if (node.ShowAll)
            {
                return AddPrefixToClasses(node.IconClass);
            }
            else
            {
                var typeEntry = node.ContentTypes
                                .Where(x => String.Equals(x.ContentTypeId, contentType.Name, StringComparison.OrdinalIgnoreCase))
                                .FirstOrDefault();

                return AddPrefixToClasses(typeEntry.IconClass);
            }
        }

        private static List<string> AddPrefixToClasses(string unprefixed)
        {
            return unprefixed?.Split(' ')
                .ToList()
                .Select(c => "icon-class-" + c)
                .ToList()
                ?? new List<string>();
        }
    }
}
