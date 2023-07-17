using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display;
using OrchardCore.ContentManagement.Records;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.Modules;
using YesSql;

namespace OrchardCore.Demo.Pages
{
    [Feature("OrchardCore.Demo.Foo")]
    public class ListModel : PageModel
    {
        private readonly IContentManager _contentManager;
        private readonly IContentItemDisplayManager _contentDisplay;
        private readonly IUpdateModelAccessor _updateModelAccessor;
        private readonly ISession _session;

        public string Title { get; private set; }
        public List<dynamic> Items { get; private set; }

        [BindProperty]
        public string Text { get; set; }

        public ListModel(
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

        public async Task OnGetAsync(string _)
        {
            var query = _session.Query<ContentItem, ContentItemIndex>()
                .With<ContentItemIndex>(x => x.ContentType == "Foo" && x.Published);

            var contentItems = await query.ListAsync();
            var updater = _updateModelAccessor.ModelUpdater;

            Items = new List<dynamic>();
            Title = "Foo List";

            foreach (var contentItem in contentItems)
            {
                Items.Add(await _contentDisplay.BuildDisplayAsync(contentItem, updater));
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var contentItem = await _contentManager.NewAsync("Foo");

            // Dynamic syntax
            contentItem.Content.TestContentPartA.Line = Text;
            await _contentManager.CreateAsync(contentItem);

            return RedirectToPage();
        }
    }
}
