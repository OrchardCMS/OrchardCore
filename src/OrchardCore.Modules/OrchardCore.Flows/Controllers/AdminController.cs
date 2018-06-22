using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Logging;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.Flows.Models;
using OrchardCore.Flows.ViewModels;

namespace OrchardCore.Flows.Controllers
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

        public ILogger Logger { get; set; }

        public async Task<IActionResult> BuildEditor(string id, string prefix, string prefixesName, string contentTypesName, string targetId, bool flowmetadata)
        {
            if (String.IsNullOrWhiteSpace(id))
            {
                return NotFound();
            }

            var contentItem = await _contentManager.NewAsync(id);

            // Does this editor need the flow metadata editor?
            if (flowmetadata)
            {
                contentItem.Weld(new FlowMetadata());
            }

            dynamic editor = await _contentItemDisplayManager.BuildEditorAsync(contentItem, this, true, htmlFieldPrefix: prefix);

            editor.PrefixesName = prefixesName;
            editor.ContentTypesName = contentTypesName;
            editor.TargetId = targetId;
            editor.Inline = true;

            var model = new BuildEditorViewModel
            {
                EditorShape = editor
            };

            if (flowmetadata)
            {
                model.EditorShape.Metadata.Alternates.Add("Widget_Edit__Flow");
            }
            else
            {
                model.EditorShape.Metadata.Alternates.Add("Widget_Edit__Bag");
            }

            return View("Display", model);
        }
    }
}
