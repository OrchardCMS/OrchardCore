using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrchardCore.Admin;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display;
using OrchardCore.Contents;
using OrchardCore.DisplayManagement.ModelBinding;
using YesSql;

namespace OrchardCore.Demo.Controllers
{
    public class ContentController : Controller, IUpdateModel
    {
        private readonly IContentItemDisplayManager _contentDisplay;
        private readonly IContentManager _contentManager;
        private readonly ISession _session;
        private readonly IAuthorizationService _authorizationService;

        public ContentController(
            IContentManager contentManager,
            IContentItemDisplayManager contentDisplay,
            ISession session,
            IAuthorizationService authorizationService)
        {
            _contentManager = contentManager;
            _contentDisplay = contentDisplay;
            _session = session;
            _authorizationService = authorizationService;
        }

        public async Task<ActionResult> Display(string contentItemId)
        {
            var contentItem = await _contentManager.GetAsync(contentItemId);

            if (contentItem == null)
            {
                return NotFound();
            }

            if (!await _authorizationService.AuthorizeAsync(User, CommonPermissions.ViewContent, contentItem))
            {
                return Forbid();
            }

            var shape = await _contentDisplay.BuildDisplayAsync(contentItem, this);
            return View(shape);
        }

        [Admin("Demo/Content/Edit", "Demo.Content.Edit")]
        public async Task<ActionResult> Edit(string contentItemId)
        {
            var contentItem = await _contentManager.GetAsync(contentItemId);

            if (contentItem == null)
            {
                return NotFound();
            }

            if (!await _authorizationService.AuthorizeAsync(User, CommonPermissions.EditContent, contentItem))
            {
                return Forbid();
            }

            var shape = await _contentDisplay.BuildEditorAsync(contentItem, this, false);
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

            if (!await _authorizationService.AuthorizeAsync(User, CommonPermissions.EditContent, contentItem))
            {
                return Forbid();
            }

            var shape = await _contentDisplay.UpdateEditorAsync(contentItem, this, false);

            if (!ModelState.IsValid)
            {
                await _session.CancelAsync();
                return View(nameof(Edit), shape);
            }

            await _session.SaveAsync(contentItem);
            return RedirectToAction(nameof(Edit), contentItemId);
        }
    }
}
