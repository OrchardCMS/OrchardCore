using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.ContentManagement.Records;
using OrchardCore.Contents.ViewModels;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.Navigation;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YesSql;
using YesSql.Services;

namespace OrchardCore.Contents.Services
{
    public class QueryFromFilterBox : IQueryFromFilterBox
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IContentManager _contentManager;
        private readonly IAuthorizationService _authorizationService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public QueryFromFilterBox(
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
        public async Task<IQuery<ContentItem, ContentItemIndex>> ApplyFilterBoxOptionsToQuery(
                                IQuery<ContentItem, ContentItemIndex> query,
                                FilterBoxViewModel filterBoxModel)
        {
            if (filterBoxModel == null)
            {
                throw new System.ArgumentNullException(nameof(filterBoxModel));
            }

            if (query == null)
            {
                throw new System.ArgumentNullException(nameof(query));
            }


            if (!string.IsNullOrEmpty(filterBoxModel.Options.TypeName))
            {
                var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(filterBoxModel.Options.TypeName);
                if (contentTypeDefinition == null)
                    throw new System.ArgumentException($"The content type {filterBoxModel.Options.TypeName} does not exist.");

                //model.TypeDisplayName = contentTypeDefinition.ToString();

                // We display a specific type even if it's not listable so that admin pages
                // can reuse the Content list page for specific types.
                query = query.With<ContentItemIndex>(x => x.ContentType == filterBoxModel.Options.TypeName);
            }
            else
            {
                var listableTypes = (await GetListableTypesAsync()).Select(t => t.Name).ToArray();
                if (listableTypes.Any())
                {
                    query = query.With<ContentItemIndex>(x => x.ContentType.IsIn(listableTypes));
                }
            }

            

            switch (filterBoxModel.Options.ContentsStatus)
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


            switch (filterBoxModel.Options.OrderBy)
            {
                case ContentsOrder.Modified:
                    query = query.OrderByDescending(x => x.ModifiedUtc);
                    break;
                case ContentsOrder.Published:
                    query = query.OrderByDescending(cr => cr.PublishedUtc);
                    break;
                case ContentsOrder.Created:
                    query = query.OrderByDescending(cr => cr.CreatedUtc);
                    break;
                default:
                    query = query.OrderByDescending(cr => cr.ModifiedUtc);
                    break;
            }

            return query;
        }


        private async Task<IEnumerable<ContentTypeDefinition>> GetListableTypesAsync()
        {
            var currentUser = _httpContextAccessor.HttpContext?.User;

            if (currentUser == null)
            {
                return null;
            }

            var listable = new List<ContentTypeDefinition>();
            foreach (var ctd in _contentDefinitionManager.ListTypeDefinitions())
            {
                if (ctd.Settings.ToObject<ContentTypeSettings>().Listable)
                {
                    var authorized = await _authorizationService.AuthorizeAsync(currentUser, Permissions.EditContent, await _contentManager.NewAsync(ctd.Name));
                    if (authorized)
                    {
                        listable.Add(ctd);
                    }
                }
            }
            return listable;
        }
    }
}
