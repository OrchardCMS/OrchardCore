using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Logging;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Display;
using Orchard.DisplayManagement.ModelBinding;
using Orchard.Widgets.Models;
using Orchard.Widgets.ViewModels;

namespace Orchard.Widgets.Controllers
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

            var contentItem = _contentManager.New(id);

            contentItem.Weld(new WidgetMetadata());

            var editor = await _contentItemDisplayManager.BuildEditorAsync(contentItem, this, htmlFieldPrefix: prefix);

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
