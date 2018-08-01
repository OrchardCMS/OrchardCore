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
using YesSql;
using YesSql.Services;

namespace OrchardCore.Contents.Services
{
    public class FilterBoxService
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IContentManager _contentManager;
        private readonly IAuthorizationService _authorizationService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public FilterBoxService(
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

                filterBoxModel.Options.TypeDisplayName = contentTypeDefinition.ToString();

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

            if (filterBoxModel.Options.OwnedByMe)
            {
                var UserName = _httpContextAccessor.HttpContext?.User.Identity.Name;
                query = query.With<ContentItemIndex>(x => x.Owner == UserName);
            }

            switch (filterBoxModel.Options.OrderBy)
            {
                case ContentsOrder.Modified:
                    query = filterBoxModel.Options.SortDirection == SortDirection.Ascending ?
                                    query.OrderBy(x => x.ModifiedUtc) : query.OrderByDescending(x => x.ModifiedUtc);
                    break;
                case ContentsOrder.Published:
                    query = filterBoxModel.Options.SortDirection == SortDirection.Ascending ?
                                    query.OrderBy(cr => cr.PublishedUtc) : query.OrderByDescending(cr => cr.PublishedUtc);
                    break;
                case ContentsOrder.Created:
                    query = filterBoxModel.Options.SortDirection == SortDirection.Ascending ?
                                    query.OrderBy(cr => cr.CreatedUtc) : query.OrderByDescending(cr => cr.CreatedUtc);
                    break;
                default:
                    query = filterBoxModel.Options.SortDirection == SortDirection.Ascending ?
                                    query.OrderBy(cr => cr.ModifiedUtc) : query.OrderByDescending(cr => cr.ModifiedUtc);
                    break;
            }

            return query;
        }


        public async Task<IEnumerable<ContentTypeDefinition>> GetCreatableTypesAsync()
        {
            var creatable = new List<ContentTypeDefinition>();

            var user = _httpContextAccessor.HttpContext?.User;
            if (user == null)
            {
                return creatable;
            }

            foreach (var ctd in _contentDefinitionManager.ListTypeDefinitions())
            {
                if (ctd.Settings.ToObject<ContentTypeSettings>().Creatable)
                {
                    var authorized = await _authorizationService.AuthorizeAsync(user, Permissions.EditContent, await _contentManager.NewAsync(ctd.Name));
                    if (authorized)
                    {
                        creatable.Add(ctd);
                    }
                }
            }
            return creatable;
        }

        public async Task<IEnumerable<ContentTypeDefinition>> GetListableTypesAsync()
        {
            var listable = new List<ContentTypeDefinition>();

            var user = _httpContextAccessor.HttpContext?.User;
            if (user == null)
            {
                return listable;
            }

            foreach (var ctd in _contentDefinitionManager.ListTypeDefinitions())
            {
                if (ctd.Settings.ToObject<ContentTypeSettings>().Listable)
                {
                    var authorized = await _authorizationService.AuthorizeAsync(user, Permissions.EditContent, await _contentManager.NewAsync(ctd.Name));
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
