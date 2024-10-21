using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentManagement.Utilities;
using OrchardCore.ContentTypes.Controllers;
using OrchardCore.ContentTypes.Services;
using OrchardCore.ContentTypes.ViewModels;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Mvc.ModelBinding;

namespace OrchardCore.ContentTypes.Editors;

public sealed class DefaultContentTypeDisplayDriver : ContentTypeDefinitionDisplayDriver
{
    private readonly IContentDefinitionService _contentDefinitionService;

    internal readonly IStringLocalizer S;

    public DefaultContentTypeDisplayDriver(
        IStringLocalizer<DefaultContentTypeDisplayDriver> stringLocalizer,
        IContentDefinitionService contentDefinitionService)
    {
        S = stringLocalizer;
        _contentDefinitionService = contentDefinitionService;
    }

    public override IDisplayResult Edit(ContentTypeDefinition contentTypeDefinition, BuildEditorContext context)
    {
        return Initialize<CreateContentViewModel>("ContentType_Edit", model =>
        {
            var isPlaceholder = context.IsNew && context.GroupId == AdminController.PlaceholderGroupId;

            model.DisplayName = isPlaceholder ? string.Empty : contentTypeDefinition.DisplayName;
            model.Name = isPlaceholder ? string.Empty : contentTypeDefinition.Name;
            model.IsNew = context.IsNew;
        }).Location("Content")
        .OnGroup(context.GroupId);
    }

    public override async Task<IDisplayResult> UpdateAsync(ContentTypeDefinition contentTypeDefinition, UpdateTypeEditorContext context)
    {
        var model = new CreateContentViewModel();

        await context.Updater.TryUpdateModelAsync(model, Prefix);

        context.Builder.DisplayedAs(model.DisplayName);

        if (string.IsNullOrWhiteSpace(model.DisplayName))
        {
            context.Updater.ModelState.AddModelError(Prefix, nameof(model.DisplayName), S["The Content Type name can't be empty."]);
        }

        if (context.IsNew)
        {
            if (string.IsNullOrWhiteSpace(model.Name))
            {
                context.Updater.ModelState.AddModelError(Prefix, nameof(model.Name), S["The Technical Name can't be empty."]);
            }
            else
            {
                if (!char.IsLetter(model.Name[0]))
                {
                    context.Updater.ModelState.AddModelError(Prefix, nameof(model.Name), S["The Technical Name must start with a letter."]);
                }

                if (!string.Equals(model.Name, model.Name.ToSafeName(), StringComparison.OrdinalIgnoreCase))
                {
                    context.Updater.ModelState.AddModelError(Prefix, nameof(model.Name), S["The Technical Name contains invalid characters."]);
                }

                var types = await _contentDefinitionService.LoadTypesAsync();

                if (model.Name != null && types.Any(t => string.Equals(t.Name.Trim(), model.Name.Trim(), StringComparison.OrdinalIgnoreCase)))
                {
                    context.Updater.ModelState.AddModelError(Prefix, nameof(model.Name), S["A type with the same Technical Name already exists."]);
                }

                if (model.Name.IsReservedContentName())
                {
                    context.Updater.ModelState.AddModelError(Prefix, nameof(model.Name), S["The Technical Name is reserved for internal use."]);
                }

                context.Builder.Named(model.Name.ToSafeName());
            }
        }

        return Edit(contentTypeDefinition, context);
    }
}
