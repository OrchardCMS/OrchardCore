using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.ContentManagement.Records;
using OrchardCore.Contents.ViewModels;
using OrchardCore.DisplayManagement.ModelBinding;
using YesSql;
using YesSql.Services;

namespace OrchardCore.Contents.Services
{
    public class DefaultContentsAdminListFilter : IContentsAdminListFilter
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IContentManager _contentManager;
        private readonly IAuthorizationService _authorizationService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public DefaultContentsAdminListFilter(
            IContentDefinitionManager contentDefinitionManager,
            IContentManager contentManager,
            IAuthorizationService authorizationService,
            IHttpContextAccessor httpContextAccessor)
        {
            _contentDefinitionManager = contentDefinitionManager;
            _contentManager = contentManager;
            _authorizationService = authorizationService;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task FilterAsync(IQuery<ContentItem> query, IUpdateModel updateModel)
        {
            var model = new ContentOptionsViewModel();
            if (await updateModel.TryUpdateModelAsync(model, "Options"))
            {
                if (!string.IsNullOrEmpty(model.DisplayText))
                {
                    query = query.With<ContentItemIndex>(x => x.DisplayText.Contains(model.DisplayText));
                }

                switch (model.ContentsStatus)
                {
                    case ContentsStatus.Published:
                        query = query.With<ContentItemIndex>(x => x.Published);
                        break;
                    case ContentsStatus.Draft:
                        query = query.With<ContentItemIndex>(x => x.Latest && !x.Published);
                        break;
                    case ContentsStatus.AllVersions:
                        query = query.With<ContentItemIndex>(x => x.Latest);
                        break;
                    default:
                        query = query.With<ContentItemIndex>(x => x.Latest);
                        break;
                }

                if (model.ContentsStatus == ContentsStatus.Owner)
                {
                    query = query.With<ContentItemIndex>(x => x.Owner == _httpContextAccessor.HttpContext.User.Identity.Name);
                }

                // Filter the creatable types.
                if (!string.IsNullOrEmpty(model.SelectedContentType))
                {
                    var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(model.SelectedContentType);
                    if (contentTypeDefinition != null)
                    {
                        // We display a specific type even if it's not listable so that admin pages
                        // can reuse the Content list page for specific types.
                        query = query.With<ContentItemIndex>(x => x.ContentType == model.SelectedContentType);
                    }
                }
                else
                {
                    var listableTypes = new List<ContentTypeDefinition>();
                    foreach (var ctd in _contentDefinitionManager.ListTypeDefinitions())
                    {
                        if (ctd.GetSettings<ContentTypeSettings>().Listable)
                        {
                            var authorized = await _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext.User, Permissions.EditContent, await _contentManager.NewAsync(ctd.Name));
                            if (authorized)
                            {
                                listableTypes.Add(ctd);
                            }
                        }
                    }
                    if (listableTypes.Any())
                    {
                        query = query.With<ContentItemIndex>(x => x.ContentType.IsIn(listableTypes.Select(t => t.Name).ToArray()));
                    }
                }

                // Apply OrderBy filters.
                query = model.OrderBy switch
                {
                    ContentsOrder.Modified => query.With<ContentItemIndex>().OrderByDescending(x => x.ModifiedUtc),
                    ContentsOrder.Published => query.With<ContentItemIndex>().OrderByDescending(cr => cr.PublishedUtc),
                    ContentsOrder.Created => query.With<ContentItemIndex>().OrderByDescending(cr => cr.CreatedUtc),
                    ContentsOrder.Title => query.With<ContentItemIndex>().OrderBy(cr => cr.DisplayText),
                    _ => query.With<ContentItemIndex>().OrderByDescending(cr => cr.ModifiedUtc),
                };
            }
        }
    }
}
