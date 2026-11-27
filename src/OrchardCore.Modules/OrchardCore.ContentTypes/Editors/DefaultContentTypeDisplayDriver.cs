using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentTypes.ViewModels;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Localization;

namespace OrchardCore.ContentTypes.Editors;

public sealed class DefaultContentTypeDisplayDriver : ContentTypeDefinitionDisplayDriver
{
    private readonly IStringLocalizerFactory _localizerFactory;

    internal readonly IStringLocalizer S;

    public DefaultContentTypeDisplayDriver(IStringLocalizer<DefaultContentTypeDisplayDriver> localizer, IStringLocalizerFactory localizerFactory)
    {
        _localizerFactory = localizerFactory;
        S = localizer;
    }

    public override IDisplayResult Edit(ContentTypeDefinition contentTypeDefinition, BuildEditorContext context)
    {
        return Initialize<ContentTypeViewModel>("ContentType_Edit", model =>
        {
            model.DisplayName = contentTypeDefinition.DisplayName;
            model.Name = contentTypeDefinition.Name;
        }).Location(l => l.Zone("Content", "1").Card(_localizerFactory.Localize(CommonLocalizationSources.General), "10"));
    }

    public override async Task<IDisplayResult> UpdateAsync(ContentTypeDefinition contentTypeDefinition, UpdateTypeEditorContext context)
    {
        var model = new ContentTypeViewModel();

        await context.Updater.TryUpdateModelAsync(model, Prefix);

        context.Builder.WithDisplayName(model.DisplayName);

        if (string.IsNullOrWhiteSpace(model.DisplayName))
        {
            context.Updater.ModelState.AddModelError("DisplayName", S["The Content Type name can't be empty."]);
        }

        return Edit(contentTypeDefinition, context);
    }
}
