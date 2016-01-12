using Microsoft.AspNet.Mvc;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Display;
using System.Threading.Tasks;
using YesSql.Core.Services;

namespace Orchard.Demo.Controllers
{
    public class ContentController : Controller
    {
        private readonly IContentDisplay _contentDisplay;
        private readonly IContentManager _contentManager;
        private readonly ISession _session;

        public ContentController(
            IContentManager contentManager,
            IContentDisplay contentDisplay,
            ISession session)
        {
            _contentManager = contentManager;
            _contentDisplay = contentDisplay;
            _session = session;
        }

        public async Task<ActionResult> Display(int id)
        {
            var contentItem = await _contentManager.Get(id);

            if (contentItem == null)
            {
                return HttpNotFound();
            }

            var shape = await _contentDisplay.BuildDisplayAsync(contentItem);
            return View(shape);
        }

        public async Task<ActionResult> Edit(int id)
        {
            var contentItem = await _contentManager.Get(id);

            if (contentItem == null)
            {
                return HttpNotFound();
            }

            var shape = await _contentDisplay.BuildEditorAsync(contentItem);
            return View(shape);
        }

        [HttpPost, ActionName("Edit")]
        public async Task<ActionResult> EditPost(int id)
        {
            var contentItem = await _contentManager.Get(id);

            if (contentItem == null)
            {
                return HttpNotFound();
            }

            await _contentDisplay.UpdateEditorAsync(contentItem);

            _session.Save(contentItem);

            return RedirectToAction("Edit");
        }
    }
}
