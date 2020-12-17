using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.ContentManagement.Records;
using OrchardCore.Contents.Security;
using OrchardCore.Contents.ViewModels;
using OrchardCore.DisplayManagement.ModelBinding;
using YesSql;
using YesSql.Services;

namespace OrchardCore.Contents.Services
{
    public class DefaultContentsAdminListFilter : IContentsAdminListFilter
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IAuthorizationService _authorizationService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IContentManager _contentManager;

        public DefaultContentsAdminListFilter(
            IContentDefinitionManager contentDefinitionManager,
            IAuthorizationService authorizationService,
            IHttpContextAccessor httpContextAccessor,
            IContentManager contentManager)
        {
            _contentDefinitionManager = contentDefinitionManager;
            _authorizationService = authorizationService;
            _httpContextAccessor = httpContextAccessor;
            _contentManager = contentManager;
        }

        public async Task FilterAsync(ContentOptionsViewModel model, IQuery<ContentItem> query, IUpdateModel updater)
        {
            var user = _httpContextAccessor.HttpContext.User;
            var userNameIdentifier = user.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!String.IsNullOrEmpty(model.DisplayText))
            {
                query.With<ContentItemIndex>(x => x.DisplayText.Contains(model.DisplayText));
            }

            switch (model.ContentsStatus)
            {
                case ContentsStatus.Published:
                    query.With<ContentItemIndex>(x => x.Published);
                    break;
                case ContentsStatus.Draft:
                    query.With<ContentItemIndex>(x => x.Latest && !x.Published);
                    break;
                case ContentsStatus.AllVersions:
                    query.With<ContentItemIndex>(x => x.Latest);
                    break;
                default:
                    query.With<ContentItemIndex>(x => x.Latest);
                    break;
            }

            var canListAllContent = await _authorizationService.AuthorizeAsync(user, Permissions.ListContent);

            // Filter the creatable types.
            if (!string.IsNullOrEmpty(model.SelectedContentType))
            {
                var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(model.SelectedContentType);
                if (contentTypeDefinition != null)
                {
                    // We display a specific type even if it's not listable so that admin pages
                    // can reuse the Content list page for specific types.
                    var contentItem = await _contentManager.NewAsync(contentTypeDefinition.Name);
                    contentItem.Owner = userNameIdentifier;

                    var hasContentListPermission = await _authorizationService.AuthorizeAsync(user, ContentTypePermissionsHelper.CreateDynamicPermission(ContentTypePermissionsHelper.PermissionTemplates[CommonPermissions.ListContent.Name], contentTypeDefinition), contentItem);
                    if (hasContentListPermission)
                    {
                        query.With<ContentItemIndex>(x => x.ContentType == model.SelectedContentType);
                    }
                    else
                    {
                        query.With<ContentItemIndex>(x => x.ContentType == model.SelectedContentType && x.Owner == userNameIdentifier);
                    }
                }
            }
            else
            {
                var listableTypes = new List<ContentTypeDefinition>();
                var authorizedContentTypes = new List<ContentTypeDefinition>();
                var unauthorizedContentTypes = new List<ContentTypeDefinition>();

                foreach (var ctd in _contentDefinitionManager.ListTypeDefinitions())
                {
                    if (ctd.GetSettings<ContentTypeSettings>().Listable)
                    {
                        // We want to list the content item if the user can edit their own items at least.
                        // It might display content items the user won't be able to edit though.
                        var contentItem = await _contentManager.NewAsync(ctd.Name);
                        contentItem.Owner = userNameIdentifier;

                        var hasEditPermission = await _authorizationService.AuthorizeAsync(user, CommonPermissions.EditContent, contentItem);
                        if (hasEditPermission)
                        {
                            listableTypes.Add(ctd);
                        }

                        if (!canListAllContent)
                        {
                            var hasContentListPermission = await _authorizationService.AuthorizeAsync(user, ContentTypePermissionsHelper.CreateDynamicPermission(ContentTypePermissionsHelper.PermissionTemplates[CommonPermissions.ListContent.Name], ctd), contentItem);
                            if (hasContentListPermission)
                            {
                                authorizedContentTypes.Add(ctd);
                            }
                            else
                            {
                                unauthorizedContentTypes.Add(ctd);
                            }
                        }
                    }
                }

                if (authorizedContentTypes.Any() && !canListAllContent)
                {
                    query.With<ContentItemIndex>().Where(x => (x.ContentType.IsIn(authorizedContentTypes.Select(t => t.Name).ToArray())) || (x.ContentType.IsIn(unauthorizedContentTypes.Select(t => t.Name).ToArray()) && x.Owner == userNameIdentifier));
                }
                else
                {
                    query.With<ContentItemIndex>(x => x.ContentType.IsIn(listableTypes.Select(t => t.Name).ToArray()));

                    // If we set the ListContent permission
                    // to false we can only view our own content and
                    // we bypass the corresponding ContentsStatus by owned content filtering
                    if (!canListAllContent)
                    {
                        query.With<ContentItemIndex>(x => x.Owner == userNameIdentifier);
                    }
                    else
                    {
                        if (model.ContentsStatus == ContentsStatus.Owner)
                        {
                            query.With<ContentItemIndex>(x => x.Owner == userNameIdentifier);
                        }
                    }
                }
            }

            // Apply OrderBy filters.
            switch (model.OrderBy)
            {
                case ContentsOrder.Modified:
                    query.With<ContentItemIndex>().OrderByDescending(x => x.ModifiedUtc);
                    break;
                case ContentsOrder.Published:
                    query.With<ContentItemIndex>().OrderByDescending(cr => cr.PublishedUtc);
                    break;
                case ContentsOrder.Created:
                    query.With<ContentItemIndex>().OrderByDescending(cr => cr.CreatedUtc);
                    break;
                case ContentsOrder.Title:
                    query.With<ContentItemIndex>().OrderBy(cr => cr.DisplayText);
                    break;
                default:
                    query.With<ContentItemIndex>().OrderByDescending(cr => cr.ModifiedUtc);
                    break;
            };
        }
    }
}
