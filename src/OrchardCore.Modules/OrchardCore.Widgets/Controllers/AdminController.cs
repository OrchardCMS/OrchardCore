using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Logging;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.Widgets.Models;
using OrchardCore.Widgets.ViewModels;

namespace OrchardCore.Widgets.Controllers
{
    public class AdminController : Controller, IUpdateModel
    {
        private readonly IContentManager _contentManager;
        private readonly IContentItemDisplayManager _contentItemDisplayManager;

        public AdminController(
            IContentManager contentManager,
            IContentItemDisplayManager contentItemDisplayManager,
            ILogger<AdminController> logger,
            IHtmlLocalizer<AdminController> localizer
            )
        {
            _contentItemDisplayManager = contentItemDisplayManager;
            _contentManager = contentManager;

            T = localizer;
            Logger = logger;
        }

        public IHtmlLocalizer T { get; }
        public dynamic New { get; set; }

        public ILogger Logger { get; set; }

        public async Task<IActionResult> BuildEditor(string id, string prefix, string prefixesName, string contentTypesName, string zonesName, string zone, string targetId)
        {
            if (String.IsNullOrWhiteSpace(id))
            {
                return NotFound();
            }

            var contentItem = await _contentManager.NewAsync(id);

            contentItem.Weld(new WidgetMetadata());

            dynamic editor = await _contentItemDisplayManager.BuildEditorAsync(contentItem, this, true, htmlFieldPrefix: prefix);

            editor.ZonesName = zonesName;
            editor.PrefixesName = prefixesName;
            editor.ContentTypesName = contentTypesName;
            editor.TargetId = targetId;
            editor.Zone = zone;
            editor.Inline = true;

            var model = new BuildEditorViewModel
            {
                EditorShape = editor
            };

            return View("Display", model);
        }
    }
}
