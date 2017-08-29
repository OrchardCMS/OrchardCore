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
using YesSql;

namespace OrchardCore.Content.Controllers
{
    public class ApiController : Controller, IUpdateModel
    {
        private readonly IContentManager _contentManager;
        private readonly IAuthorizationService _authorizationService;
        private readonly ISession _session;
        private readonly ITypeActivatorFactory<ContentPart> _contentPartFactory;
        private readonly IEnumerable<IContentApiFilter> _contentAdminFilters;

        public ApiController(
            IContentManager contentManager,
            IAuthorizationService authorizationService,
            ISession session,
            ITypeActivatorFactory<ContentPart> contentPartFactory,
            IEnumerable<IContentApiFilter> contentAdminFilters,
            ILogger<ApiController> logger)
        {
            _authorizationService = authorizationService;
            _contentManager = contentManager;
            _session = session;
            _contentPartFactory = contentPartFactory;
            _contentAdminFilters = contentAdminFilters;

            Logger = logger;
        }

        public ILogger Logger { get; set; }

        public async Task<IActionResult> GetByContentType(string contentType, ContentsStatus contentsStatus = ContentsStatus.Published)
        {
            //var siteSettings = await _siteService.GetSiteSettingsAsync();
            //Pager pager = new Pager(pagerParameters, siteSettings.PageSize);

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

            var contentItems = await query.ListAsync();

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

            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ViewContent, contentItem))
            {
                return Unauthorized();
            }

            return new ObjectResult(contentItem);
        }

        public async Task<IActionResult> GetByContentByRelationship(string contentType, string contentItemId, string nestedContentType, ContentsStatus contentsStatus = ContentsStatus.Published)
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

            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ViewContent, contentItem))
            {
                return Unauthorized();
            }

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

            var contentItems = await query.ListAsync();

            return new ObjectResult(contentItems);
        }
    }
}
