using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using OrchardCore.Admin;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.ContentManagement.Records;
using OrchardCore.Contents.Services;
using OrchardCore.Contents.ViewModels;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Navigation;
using OrchardCore.Routing;
using OrchardCore.Settings;
using YesSql;
using YesSql.Services;

namespace OrchardCore.Contents.Controllers
{
    [Admin]
    public class HistoryController : Controller
    {
        private readonly IContentManager _contentManager;
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly ISiteService _siteService;
        private readonly YesSql.ISession _session;
        private readonly IContentItemDisplayManager _contentItemDisplayManager;
        private readonly INotifier _notifier;
        private readonly IAuthorizationService _authorizationService;
        private readonly IDisplayManager<ContentOptionsViewModel> _contentOptionsDisplayManager;
        private readonly IContentsAdminListQueryService _contentsAdminListQueryService;
        private readonly IHtmlLocalizer H;
        private readonly IStringLocalizer S;
        private readonly IUpdateModelAccessor _updateModelAccessor;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IShapeFactory _shapeFactory;
        private readonly dynamic New;
        private readonly ILogger _logger;

        public HistoryController(
            IAuthorizationService authorizationService,
            IContentManager contentManager,
            IContentItemDisplayManager contentItemDisplayManager,
            IContentDefinitionManager contentDefinitionManager,
            ISiteService siteService,
            INotifier notifier,
            YesSql.ISession session,
            IShapeFactory shapeFactory,
            IDisplayManager<ContentOptionsViewModel> contentOptionsDisplayManager,
            IContentsAdminListQueryService contentsAdminListQueryService,
            ILogger<AdminController> logger,
            IHtmlLocalizer<AdminController> htmlLocalizer,
            IStringLocalizer<AdminController> stringLocalizer,
            IUpdateModelAccessor updateModelAccessor,
            IHttpContextAccessor httpContextAccessor)
        {
            _authorizationService = authorizationService;
            _notifier = notifier;
            _contentItemDisplayManager = contentItemDisplayManager;
            _session = session;
            _siteService = siteService;
            _contentManager = contentManager;
            _contentDefinitionManager = contentDefinitionManager;
            _updateModelAccessor = updateModelAccessor;
            _httpContextAccessor = httpContextAccessor;
            _contentOptionsDisplayManager = contentOptionsDisplayManager;
            _contentsAdminListQueryService = contentsAdminListQueryService;

            H = htmlLocalizer;
            S = stringLocalizer;
            _shapeFactory = shapeFactory;
            New = shapeFactory;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index(PagerParameters pagerParameters, string contentItemId, string oldItemVersionId = null, string newItemVersionId = null)
        {
            // var context = _httpContextAccessor.HttpContext;
            // var contentTypeDefinitions = _contentDefinitionManager.ListTypeDefinitions()
            //         .Where(ctd => ctd.GetSettings<ContentTypeSettings>().Creatable)
            //         .OrderBy(ctd => ctd.DisplayName);

            // if (!await _authorizationService.AuthorizeContentTypeDefinitionsAsync(User, CommonPermissions.EditContent, contentTypeDefinitions, _contentManager))
            // {
            //     return Forbid();
            // }

            var siteSettings = await _siteService.GetSiteSettingsAsync();
            var pager = new Pager(pagerParameters, siteSettings.PageSize);

            var query = _session.Query<ContentItem, ContentItemIndex>(x => x.ContentItemId == contentItemId)
                .OrderByDescending(x => x.ModifiedUtc);

            // With the model populated we filter the query, allowing the filters to alter the model.
            // var query = await _contentsAdminListQueryService.QueryAsync(model.Options, _updateModelAccessor.ModelUpdater);

            var maxPagedCount = siteSettings.MaxPagedCount;
            if (maxPagedCount > 0 && pager.PageSize > maxPagedCount)
            {
                pager.PageSize = maxPagedCount;
            }

            // Populate route values to maintain previous route data when generating page links.
            // await _contentOptionsDisplayManager.UpdateEditorAsync(model.Options, _updateModelAccessor.ModelUpdater, false);

            // var routeData = new RouteData(model.Options.RouteValues);

            var pagerShape = (await New.Pager(pager)).TotalItemCount(maxPagedCount > 0 ? maxPagedCount : await query.CountAsync());//.RouteData(routeData);

            // Load items so that loading handlers are invoked.
            var pageOfContentItems = (await query.Skip(pager.GetStartIndex()).Take(pager.PageSize).ListAsync()).ToArray();

            // We prepare the content items SummaryAdmin shape
            var contentItemSummaries = new List<IShape>();
            // string nextVersionId = String.Empty;
            for(var i = 0; i < pageOfContentItems.Length; i++)
            {
                var contentItem = pageOfContentItems[i];
                var shape = await _contentItemDisplayManager.BuildDisplayAsync(contentItem, _updateModelAccessor.ModelUpdater, "HistoryAdmin");
                if (pageOfContentItems.Length > i + 1)
                {
                    shape.Properties.Add("NextVersionId", pageOfContentItems[i + 1].ContentItemVersionId);
                }
                contentItemSummaries.Add(shape);
            }

            // Populate options pager summary values.
            var startIndex = (pagerShape.Page - 1) * (pagerShape.PageSize) + 1;
            // model.Options.StartIndex = startIndex;
            // model.Options.EndIndex = startIndex + contentItemSummaries.Count - 1;
            // model.Options.ContentItemsCount = contentItemSummaries.Count;
            // model.Options.TotalItemCount = pagerShape.TotalItemCount;

            // var header = await _contentOptionsDisplayManager.BuildEditorAsync(model.Options, _updateModelAccessor.ModelUpdater, false);

            // var shapeViewModel = await _shapeFactory.CreateAsync<ListContentsViewModel>("ContentsAdminList", viewModel =>
            // {
            //     viewModel.ContentItems = contentItemSummaries;
            //     viewModel.Pager = pagerShape;
            //     viewModel.Options = model.Options;
            //     viewModel.Header = header;
            // });

            var model = new HistoryIndexViewModel{ ContentItems = contentItemSummaries};

            if (!String.IsNullOrEmpty(oldItemVersionId))
            {
                var oldContentItem = pageOfContentItems.FirstOrDefault(x => x.ContentItemVersionId == oldItemVersionId);
                model.OldContentItem = oldContentItem;
            }
            else
            {
                model.OldContentItem = pageOfContentItems.Skip(1).FirstOrDefault();
            }

            if (!String.IsNullOrEmpty(newItemVersionId))
            {
                var newContentItem = pageOfContentItems.FirstOrDefault(x => x.ContentItemVersionId == newItemVersionId);
                model.NewContentItem = newContentItem;
            }
            else
            {
                model.NewContentItem = pageOfContentItems.FirstOrDefault();
            }            

            return View(model);
        }
    }
}
