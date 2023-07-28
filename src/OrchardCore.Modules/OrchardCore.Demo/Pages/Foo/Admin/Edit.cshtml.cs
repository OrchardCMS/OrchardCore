using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.Modules;
using YesSql;

namespace OrchardCore.Demo.Pages
{
    [Feature("OrchardCore.Demo.Foo")]
    public class EditModel : PageModel
    {
        private readonly IContentManager _contentManager;
        private readonly IContentItemDisplayManager _contentDisplay;
        private readonly IUpdateModelAccessor _updateModelAccessor;
        private readonly ISession _session;

        public EditModel(
            IContentManager contentManager,
            IContentItemDisplayManager contentDisplay,
            IUpdateModelAccessor updateModelAccessor,
            ISession session)
        {
            _contentManager = contentManager;
            _contentDisplay = contentDisplay;
            _updateModelAccessor = updateModelAccessor;
            _session = session;
        }

        [BindProperty(SupportsGet = true)]
        public string Id { get; set; }

        [Required]
        [BindProperty]
        public string Text { get; set; }

        public string Title { get; set; }
        public string Submit { get; set; }

        public async Task<ActionResult> OnGetAsync()
        {
            var contentItem = await _contentManager.GetAsync(Id);

            if (contentItem == null)
            {
                return NotFound();
            }

            Title = contentItem.DisplayText ?? "Foo Title";
            Text = contentItem.Content.TestContentPartA.Line;

            return Page();
        }

        public async Task<IActionResult> OnPostDelete()
        {
            var contentItem = await _contentManager.GetAsync(Id);
            if (contentItem == null)
            {
                return NotFound();
            }
            await _contentManager.RemoveAsync(contentItem);

            return RedirectToPage("/Foo/List");
        }

        public async Task<IActionResult> OnPostUpdate()
        {
            var contentItem = await _contentManager.GetAsync(Id);

            if (contentItem == null)
            {
                return NotFound();
            }

            var updater = _updateModelAccessor.ModelUpdater;
            _ = await _contentDisplay.UpdateEditorAsync(contentItem, updater, false);

            if (!ModelState.IsValid)
            {
                await _session.CancelAsync();
                return Page();
            }

            contentItem.Content.TestContentPartA.Line = Text;
            _session.Save(contentItem);

            return RedirectToPage("/Foo/List");
        }
    }
}
