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

    /// <summary>
    /// Renders the content types to select as a searchable list that shows the selected content types as tags, with
    /// select all. When <paramref name="displayMode"/> is <c>Picker</c>, its placeholder reads "Any content type",
    /// for a filter where no selection means any content type; when it is <c>Checkboxes</c>, they are checkboxes.
    /// </summary>
    public async Task<IViewComponentResult> InvokeAsync(IEnumerable<string> selectedContentTypes, string htmlName, string stereotype, string displayMode)
    {
        var contentTypes = await ContentTypeSelection.BuildAsync(_contentDefinitionManager, selectedContentTypes ?? []);

        if (!string.IsNullOrEmpty(stereotype))
        {
            contentTypes = contentTypes
                .Where(x => x.ContentTypeDefinition.StereotypeEquals(stereotype))
                .ToArray();
        }

        var model = new SelectContentTypesViewModel
        {
            HtmlName = htmlName,
            ContentTypeSelections = contentTypes,
        };

        return displayMode switch
        {
            "Picker" => View("Picker", model),
            "Checkboxes" => View("Checkboxes", model),
            _ => View(model),
        };
    }
}
