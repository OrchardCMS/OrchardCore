using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using OrchardCore.Admin;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display;
using OrchardCore.DisplayManagement.ModelBinding;
using YesSql;

namespace OrchardCore.Demo.Controllers
{
    public class ContentController : Controller
    {
        private readonly IContentItemDisplayManager _contentDisplay;
        private readonly IContentManager _contentManager;
        private readonly ISession _session;
        private readonly IUpdateModelAccessor _updateModelAccessor;

        public ContentController(
            IContentManager contentManager,
            IContentItemDisplayManager contentDisplay,
            ISession session,
            IUpdateModelAccessor updateModelAccessor)
        {
            _contentManager = contentManager;
            _contentDisplay = contentDisplay;
            _session = session;
            _updateModelAccessor = updateModelAccessor;
        }

        public async Task<ActionResult> Display(string contentItemId)
        {
            var contentItem = await _contentManager.GetAsync(contentItemId);

            if (contentItem == null)
            {
                return NotFound();
            }

            var shape = await _contentDisplay.BuildDisplayAsync(contentItem, _updateModelAccessor.ModelUpdater);
            return View(shape);
        }

        [Admin]
        public async Task<ActionResult> Edit(string contentItemId)
        {
            var contentItem = await _contentManager.GetAsync(contentItemId);

            if (contentItem == null)
            {
                return NotFound();
            }

            var shape = await _contentDisplay.BuildEditorAsync(contentItem, _updateModelAccessor.ModelUpdater, false);
            return View(shape);
        }

        [Admin, HttpPost, ActionName("Edit")]
        public async Task<ActionResult> EditPost(string contentItemId)
        {
            var contentItem = await _contentManager.GetAsync(contentItemId);

            if (contentItem == null)
            {
                return NotFound();
            }

            var shape = await _contentDisplay.UpdateEditorAsync(contentItem, _updateModelAccessor.ModelUpdater, false);

            if (!ModelState.IsValid)
            {
                _session.Cancel();
                return View("Edit", shape);
            }

            _session.Save(contentItem);
            return RedirectToAction("Edit", contentItemId);
        }
    }
}
