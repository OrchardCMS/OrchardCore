using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using OrchardCore.Modules;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Mvc.ViewFeatures.Internal;
using Microsoft.Extensions.Logging;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Settings;
using YesSql;

namespace OrchardCore.ContentPreview.Controllers
{
    public class PreviewController : Controller, IUpdateModel
    {
        private readonly IContentManager _contentManager;
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly ISiteService _siteService;
        private readonly ISession _session;
        private readonly IContentItemDisplayManager _contentItemDisplayManager;
        private readonly INotifier _notifier;
        private readonly IAuthorizationService _authorizationService;
        private readonly IClock _clock;

        public PreviewController(
            IContentManager contentManager,
            IContentItemDisplayManager contentItemDisplayManager,
            IContentDefinitionManager contentDefinitionManager,
            ISiteService siteService,
            INotifier notifier,
            ISession session,
            IShapeFactory shapeFactory,
            ILogger<PreviewController> logger,
            IHtmlLocalizer<PreviewController> localizer,
            IAuthorizationService authorizationService,
            IClock clock
            )
        {
            _authorizationService = authorizationService;
            _clock = clock;
            _notifier = notifier;
            _contentItemDisplayManager = contentItemDisplayManager;
            _session = session;
            _siteService = siteService;
            _contentManager = contentManager;
            _contentDefinitionManager = contentDefinitionManager;

            T = localizer;
            New = shapeFactory;
            Logger = logger;
        }

        public IHtmlLocalizer T { get; }
        public dynamic New { get; set; }

        public ILogger Logger { get; set; }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Render()
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ContentPreview))
            {
                return Unauthorized();
            }

            var contentItemType = Request.Form["ContentItemType"];
            var contentItem = await _contentManager.NewAsync(contentItemType);

            // Assign the ids from the currently edited item so that validation thinks
            // it's working on the same item. For instance if drivers are checking name unicity
            // they need to think this is the same existing item (AutoroutePart).

            var contentItemId = Request.Form["PreviewContentItemId"];
            var contentItemVersionId = Request.Form["PreviewContentItemVersionId"];
            int.TryParse(Request.Form["PreviewId"], out var contentId);

            contentItem.Id = contentId;
            contentItem.ContentItemId = contentItemId;
            contentItem.ContentItemVersionId = contentItemVersionId;
            contentItem.CreatedUtc = _clock.UtcNow;
            contentItem.ModifiedUtc = _clock.UtcNow;
            contentItem.PublishedUtc = _clock.UtcNow;

            // TODO: we should probably get this value from the main editor as it might impact validators
            var model = await _contentItemDisplayManager.UpdateEditorAsync(contentItem, this, true);

            if (!ModelState.IsValid)
            {
                var errors = new List<string>();
                foreach (var modelState in ValidationHelpers.GetModelStateList(ViewData, false))
                {
                    for (var i = 0; i < modelState.Errors.Count; i++)
                    {
                        var modelError = modelState.Errors[i];
                        var errorText = ValidationHelpers.GetModelErrorMessageOrDefault(modelError);
                        errors.Add(errorText);
                    }
                }

                return StatusCode(500, new { errors = errors });
            }

            model = await _contentItemDisplayManager.BuildDisplayAsync(contentItem, this, "Detail");

            return View(model);
        }
    }
}
