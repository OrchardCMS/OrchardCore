using Microsoft.AspNetCore.Mvc;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Display;
using Orchard.DisplayManagement.ModelBinding;
using System.Threading.Tasks;
using YesSql;
using Orchard.Admin;

namespace Orchard.Demo.Controllers
{
    public class ContentController : Controller, IUpdateModel
    {
        private readonly IContentItemDisplayManager _contentDisplay;
        private readonly IContentManager _contentManager;
        private readonly ISession _session;

        public ContentController(
            IContentManager contentManager,
            IContentItemDisplayManager contentDisplay,
            ISession session)
        {
            _contentManager = contentManager;
            _contentDisplay = contentDisplay;
            _session = session;
        }

        public async Task<ActionResult> Display(string contentItemId)
        {
            var contentItem = await _contentManager.GetAsync(contentItemId);

            if (contentItem == null)
            {
                return NotFound();
            }

            var shape = await _contentDisplay.BuildDisplayAsync(contentItem, this);
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

            var shape = await _contentDisplay.BuildEditorAsync(contentItem, this);
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

            var shape = await _contentDisplay.UpdateEditorAsync(contentItem, this);

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
