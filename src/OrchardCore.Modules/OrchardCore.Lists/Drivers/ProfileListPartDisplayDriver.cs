using System;
using System.Collections.Generic;
using System.Linq;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Lists.Models;
using OrchardCore.Lists.ViewModels;

namespace OrchardCore.Lists.Drivers;

public class ProfileListPartDisplayDriver : ContentPartDisplayDriver<ListPart>
{
    private readonly IContentDefinitionManager _contentDefinitionManager;

    public ProfileListPartDisplayDriver(IContentDefinitionManager contentDefinitionManager)
    {
        _contentDefinitionManager = contentDefinitionManager;
    }

    public override IDisplayResult Display(ListPart part, BuildPartDisplayContext context)
    {
        if (!String.Equals("DetailAdmin", context.DisplayType, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        return Initialize<ProfileHeaderViewModel>("ProfileHeader", model =>
        {
            var settings = context.TypePartDefinition.GetSettings<ListPartSettings>();

            model.ContainerContentItem = part.ContentItem;
            model.ContainedContentTypeDefinitions = GetContainedContentTypes(settings).ToArray();
            model.EnableOrdering = settings.EnableOrdering;
        }).Location("Content:1");
    }

    public override IDisplayResult Edit(ListPart part, BuildPartEditorContext context)
    {
        if (context.IsNew)
        {
            return null;
        }

        return Initialize<ProfileHeaderViewModel>("ProfileHeader", model =>
        {
            var settings = context.TypePartDefinition.GetSettings<ListPartSettings>();

            model.ContainerContentItem = part.ContentItem;
            model.ContainedContentTypeDefinitions = GetContainedContentTypes(settings).ToArray();
            model.EnableOrdering = settings.EnableOrdering;
        }).Location("Content:1");
    }

    private IEnumerable<ContentTypeDefinition> GetContainedContentTypes(ListPartSettings settings)
    {
        var contentTypes = settings.ContainedContentTypes ?? Enumerable.Empty<string>();

        return contentTypes.Select(contentType => _contentDefinitionManager.GetTypeDefinition(contentType));
    }
}
