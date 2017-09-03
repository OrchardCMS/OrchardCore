using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Modules;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Records;
using OrchardCore.Contents;
using OrchardCore.Contents.Services;
using OrchardCore.Contents.ViewModels;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.Navigation;
using OrchardCore.Settings;
using YesSql;

namespace OrchardCore.Content.Controllers
{
    public class ApiController : Controller, IUpdateModel
    {
        private readonly IContentManager _contentManager;
        private readonly ISiteService _siteService;
        private readonly IAuthorizationService _authorizationService;
        private readonly ISession _session;
        private readonly ITypeActivatorFactory<ContentPart> _contentPartFactory;
        private readonly IEnumerable<IContentApiFilter> _contentAdminFilters;

        public ApiController(
            IContentManager contentManager,
            ISiteService siteService,
            IAuthorizationService authorizationService,
            ISession session,
            ITypeActivatorFactory<ContentPart> contentPartFactory,
            IEnumerable<IContentApiFilter> contentAdminFilters,
            ILogger<ApiController> logger)
        {
            _contentManager = contentManager;
            _siteService = siteService;
            _authorizationService = authorizationService;
            _session = session;
            _contentPartFactory = contentPartFactory;
            _contentAdminFilters = contentAdminFilters;

            Logger = logger;
        }

        public ILogger Logger { get; set; }

        public async Task<IActionResult> GetByContentType(string contentType, PagerParameters pagerParameters, ContentsStatus contentsStatus = ContentsStatus.Published)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Orchard.Contents.Permissions.ViewContent))
            {
                return Unauthorized();
            }

            var siteSettings = await _siteService.GetSiteSettingsAsync();
            Pager pager = new Pager(pagerParameters, siteSettings.PageSize);

            var query = _session
                .Query<ContentItem, ContentItemIndex>(cix => cix.ContentType == contentType);

            switch (contentsStatus)
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

            var maxPagedCount = siteSettings.MaxPagedCount;
            if (maxPagedCount > 0 && pager.PageSize > maxPagedCount)
                pager.PageSize = maxPagedCount;

            //var pagerShape = New.Pager(pager).TotalItemCount(maxPagedCount > 0 ? maxPagedCount : await query.CountAsync());
            var pageOfContentItems = await query.Skip(pager.GetStartIndex()).Take(pager.PageSize).ListAsync();

            var contentItems = pageOfContentItems;

            if (contentItems == null)
            {
                return NotFound();
            }

            //if (!await _authorizationService.AuthorizeAsync(User, Permissions.ViewContent, contentItem))
            //{
            //    return Unauthorized();
            //}

            return new ObjectResult(contentItems);
        }

        public async Task<IActionResult> GetByContentTypeAndId(string contentType, string contentItemId, ContentsStatus contentsStatus = ContentsStatus.Published)
        {
            var contentItem = await _contentManager.GetAsync(contentItemId);

            if (contentItem == null)
            {
                return NotFound();
            }

            if (contentType != contentItem.ContentType)
            {
                return NotFound();
            }

            if (!await _authorizationService.AuthorizeAsync(User, Orchard.Contents.Permissions.ViewContent, contentItem))
            {
                return Unauthorized();
            }

            return new ObjectResult(contentItem);
        }

        public async Task<IActionResult> GetByContentByRelationship(string contentType, string contentItemId, string nestedContentType, PagerParameters pagerParameters, ContentsStatus contentsStatus = ContentsStatus.Published)
        {
            var contentItem = await _contentManager.GetAsync(contentItemId);

            if (contentItem == null)
            {
                return NotFound();
            }

            if (contentType != contentItem.ContentType)
            {
                return NotFound();
            }

            if (!await _authorizationService.AuthorizeAsync(User, Orchard.Contents.Permissions.ViewContent, contentItem))
            {
                return Unauthorized();
            }

            var siteSettings = await _siteService.GetSiteSettingsAsync();
            Pager pager = new Pager(pagerParameters, siteSettings.PageSize);

            var query = _session
                .Query<ContentItem, ContentItemIndex>(cix => cix.ContentType == nestedContentType);

            switch (contentsStatus)
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

            await _contentAdminFilters.InvokeAsync(x => x.FilterAsync(query, contentItemId), Logger);

            var maxPagedCount = siteSettings.MaxPagedCount;
            if (maxPagedCount > 0 && pager.PageSize > maxPagedCount)
                pager.PageSize = maxPagedCount;

            var pageOfContentItems = await query.Skip(pager.GetStartIndex()).Take(pager.PageSize).ListAsync();

            var contentItems = pageOfContentItems;

            return new ObjectResult(contentItems);
        }
    }
}
