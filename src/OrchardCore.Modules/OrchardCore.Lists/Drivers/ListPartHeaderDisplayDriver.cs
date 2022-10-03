using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Lists.Models;
using OrchardCore.Lists.ViewModels;

namespace OrchardCore.Lists.Drivers;

public class ListPartHeaderDisplayDriver : ContentPartDisplayDriver<ListPart>
{
    private readonly IContentDefinitionManager _contentDefinitionManager;

    public ListPartHeaderDisplayDriver(IContentDefinitionManager contentDefinitionManager)
    {
        _contentDefinitionManager = contentDefinitionManager;
    }

    public override IDisplayResult Display(ListPart part, BuildPartDisplayContext context)
    {
        var settings = context.TypePartDefinition.GetSettings<ListPartSettings>();

        return Initialize<ListPartHeaderViewModel>("ListPartHeader", model =>
        {
            model.ContainerContentItem = part.ContentItem;
            model.ContainedContentTypeDefinitions = GetContainedContentTypes(settings).ToArray();
            model.EnableOrdering = settings.EnableOrdering;
        }).Location("DetailAdmin", "Content:1")
        .RenderWhen(() => Task.FromResult(settings.ShowHeader));
    }

    public override IDisplayResult Edit(ListPart part, BuildPartEditorContext context)
    {
        var settings = context.TypePartDefinition.GetSettings<ListPartSettings>();

        return Initialize<ListPartHeaderViewModel>("ListPartHeader", model =>
        {
            model.ContainerContentItem = part.ContentItem;
            model.ContainedContentTypeDefinitions = GetContainedContentTypes(settings).ToArray();
            model.EnableOrdering = settings.EnableOrdering;
        }).Location("Content:1")
        .RenderWhen(() => Task.FromResult(context.IsNew && settings.ShowHeader));
    }

    private IEnumerable<ContentTypeDefinition> GetContainedContentTypes(ListPartSettings settings)
    {
        var contentTypes = settings.ContainedContentTypes ?? Enumerable.Empty<string>();

        return contentTypes.Select(contentType => _contentDefinitionManager.GetTypeDefinition(contentType));
    }
}
