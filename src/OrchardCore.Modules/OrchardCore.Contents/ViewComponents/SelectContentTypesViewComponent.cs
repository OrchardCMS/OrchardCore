using Microsoft.AspNetCore.Mvc;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.Contents.ViewModels;

namespace OrchardCore.Contents.ViewComponents;

public class SelectContentTypesViewComponent : ViewComponent
{
    private readonly IContentDefinitionManager _contentDefinitionManager;

    public SelectContentTypesViewComponent(IContentDefinitionManager contentDefinitionManager)
    {
        _contentDefinitionManager = contentDefinitionManager;
    }

    public async Task<IViewComponentResult> InvokeAsync(IEnumerable<string> selectedContentTypes, string htmlName, string stereotype)
    {
        selectedContentTypes ??= Array.Empty<string>();

        var contentTypes = await ContentTypeSelection.BuildAsync(_contentDefinitionManager, selectedContentTypes);

        if (!string.IsNullOrEmpty(stereotype))
        {
            contentTypes = contentTypes
                .Where(x => x.ContentTypeDefinition.GetStereotype() == stereotype)
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
