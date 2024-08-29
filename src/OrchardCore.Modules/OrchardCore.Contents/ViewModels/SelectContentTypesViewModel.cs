using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;

namespace OrchardCore.Contents.ViewModels;

public class SelectContentTypesViewModel
{
    public string HtmlName { get; set; }
    public ContentTypeSelection[] ContentTypeSelections { get; set; }
}

public class ContentTypeSelection
{
    public bool IsSelected { get; set; }
    public ContentTypeDefinition ContentTypeDefinition { get; set; }

    public static async Task<ContentTypeSelection[]> BuildAsync(IContentDefinitionManager contentDefinitionManager, IEnumerable<string> selectedContentTypes)
    {
        var contentTypes = (await contentDefinitionManager.ListTypeDefinitionsAsync())
            .Select(x =>
                new ContentTypeSelection
                {
                    IsSelected = selectedContentTypes.Contains(x.Name),
                    ContentTypeDefinition = x
                })
            .OrderBy(type => type.ContentTypeDefinition.DisplayName)
            .ToArray();

        return contentTypes;
    }
}
