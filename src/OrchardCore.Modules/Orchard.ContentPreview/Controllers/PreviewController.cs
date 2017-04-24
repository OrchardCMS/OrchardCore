using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Logging;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Display;
using Orchard.ContentManagement.MetaData;
using Orchard.DisplayManagement;
using Orchard.DisplayManagement.ModelBinding;
using Orchard.DisplayManagement.Notify;
using Orchard.Settings;
using YesSql;

namespace Orchard.ContentPreview.Controllers
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
            IAuthorizationService authorizationService
            )
        {
            _authorizationService = authorizationService;
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

        [HttpPost]
        public async Task<IActionResult> Index(string id)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ContentPreview))
            {
                return Unauthorized();
            }

            var contentItem = _contentManager.New(id);
            
            var model = await _contentItemDisplayManager.UpdateEditorAsync(contentItem, this);

            if (!ModelState.IsValid)
            {
                return StatusCode(500);
            }

            model = await _contentItemDisplayManager.BuildDisplayAsync(contentItem, this, "Detail");

            return View(model);
        }
    }
}
