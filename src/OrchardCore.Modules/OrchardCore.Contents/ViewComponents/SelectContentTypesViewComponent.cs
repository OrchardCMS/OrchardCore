using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.Contents.ViewModels;

namespace OrchardCore.Contents.ViewComponents
{
    public class SelectContentTypesViewComponent : ViewComponent
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;

        public SelectContentTypesViewComponent(IContentDefinitionManager contentDefinitionManager)
        {
            _contentDefinitionManager = contentDefinitionManager;
        }

        public IViewComponentResult Invoke(IEnumerable<string> selectedContentTypes, string htmlName, string stereotype)
        {
            if (selectedContentTypes == null)
            {
                selectedContentTypes = new string[0];
            }

            var contentTypes = ContentTypeSelection.Build(_contentDefinitionManager, selectedContentTypes);

            if (!String.IsNullOrEmpty(stereotype))
            {
                contentTypes = contentTypes
                    .Where(x => x.ContentTypeDefinition.GetSettings<ContentTypeSettings>().Stereotype == stereotype)
                    .ToArray();
            }

            var model = new SelectContentTypesViewModel
            {
                HtmlName = htmlName,
                ContentTypeSelections = contentTypes
            };

            return View(model);
        }
    }
}
