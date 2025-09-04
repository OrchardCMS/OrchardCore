using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Lists.Models;
using OrchardCore.Lists.Services;
using OrchardCore.Lists.ViewModels;

namespace OrchardCore.Lists.Settings;

public sealed class ListPartSettingsDisplayDriver : ContentTypePartDefinitionDisplayDriver<ListPart>
{
    private readonly IContentDefinitionManager _contentDefinitionManager;
    private readonly IContainerService _containerService;

    internal readonly IStringLocalizer S;

    public ListPartSettingsDisplayDriver(
        IContentDefinitionManager contentDefinitionManager,
        IContainerService containerService,
        IStringLocalizer<ListPartSettingsDisplayDriver> localizer)
    {
        _contentDefinitionManager = contentDefinitionManager;
        _containerService = containerService;
        S = localizer;
    }

    public override IDisplayResult Edit(ContentTypePartDefinition contentTypePartDefinition, BuildEditorContext context)
    {
        return Initialize<ListPartSettingsViewModel>("ListPartSettings_Edit", async model =>
        {
            model.ListPartSettings = contentTypePartDefinition.GetSettings<ListPartSettings>();
            model.PageSize = model.ListPartSettings.PageSize;
            model.EnableOrdering = model.ListPartSettings.EnableOrdering;
            model.ContainedContentTypes = model.ListPartSettings.ContainedContentTypes;
            model.ShowHeader = model.ListPartSettings.ShowHeader;
            model.ContentTypes = [];

            foreach (var contentTypeDefinition in await _contentDefinitionManager.ListTypeDefinitionsAsync())
            {
                model.ContentTypes.Add(contentTypeDefinition.Name, contentTypeDefinition.DisplayName);
            }
        }).Location("Content");
    }

    public override async Task<IDisplayResult> UpdateAsync(ContentTypePartDefinition contentTypePartDefinition, UpdateTypePartEditorContext context)
    {
        var settings = contentTypePartDefinition.GetSettings<ListPartSettings>();

        var model = new ListPartSettingsViewModel();

        await context.Updater.TryUpdateModelAsync(model, Prefix, m => m.ContainedContentTypes, m => m.PageSize, m => m.EnableOrdering, m => m.ShowHeader);

        if (model.ContainedContentTypes == null || model.ContainedContentTypes.Length == 0)
        {
            context.Updater.ModelState.AddModelError(nameof(model.ContainedContentTypes), S["At least one content type must be selected."]);
        }
        else
        {
            context.Builder.WithSettings(new ListPartSettings
            {
                PageSize = model.PageSize,
                EnableOrdering = model.EnableOrdering,
                ContainedContentTypes = model.ContainedContentTypes,
                ShowHeader = model.ShowHeader,
            });

            // Update order of existing content if enable ordering has been turned on.
            if (settings.EnableOrdering != model.EnableOrdering && model.EnableOrdering == true)
            {
                await _containerService.SetInitialOrder(contentTypePartDefinition.ContentTypeDefinition.Name);
            }
        }

        return Edit(contentTypePartDefinition, context);
    }
}
