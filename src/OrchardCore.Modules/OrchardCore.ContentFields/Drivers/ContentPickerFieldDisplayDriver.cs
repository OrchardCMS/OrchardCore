using Fluid;
using Fluid.Values;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentFields.Settings;
using OrchardCore.ContentFields.ViewModels;
using OrchardCore.ContentLocalization;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.Contents;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Liquid;
using OrchardCore.Localization;
using OrchardCore.Mvc.ModelBinding;

namespace OrchardCore.ContentFields.Drivers;

public sealed class ContentPickerFieldDisplayDriver : ContentFieldDisplayDriver<ContentPickerField>
{
    private readonly IContentManager _contentManager;
    private readonly ILiquidTemplateManager _templateManager;
    private readonly IAuthorizationService _authorizationService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    internal readonly IStringLocalizer S;

    public ContentPickerFieldDisplayDriver(
        IContentManager contentManager,
        IStringLocalizer<ContentPickerFieldDisplayDriver> localizer,
        ILiquidTemplateManager templateManager,
        IAuthorizationService authorizationService,
        IHttpContextAccessor httpContextAccessor)
    {
        _contentManager = contentManager;
        S = localizer;
        _templateManager = templateManager;
        _authorizationService = authorizationService;
        _httpContextAccessor = httpContextAccessor;
    }

    public override IDisplayResult Display(ContentPickerField field, BuildFieldDisplayContext fieldDisplayContext)
    {
        return Initialize<DisplayContentPickerFieldViewModel>(GetDisplayShapeType(fieldDisplayContext), model =>
        {
            model.Field = field;
            model.Part = fieldDisplayContext.ContentPart;
            model.PartFieldDefinition = fieldDisplayContext.PartFieldDefinition;
        })
        .Location("Detail", "Content")
        .Location("Summary", "Content");
    }

    public override IDisplayResult Edit(ContentPickerField field, BuildFieldEditorContext context)
    {
        return Initialize<EditContentPickerFieldViewModel>(GetEditorShapeType(context), async model =>
        {
            model.ContentItemIds = string.Join(",", field.ContentItemIds);

            model.Field = field;
            model.Part = context.ContentPart;
            model.PartFieldDefinition = context.PartFieldDefinition;

            model.SelectedItems = [];
            var settings = context.PartFieldDefinition.GetSettings<ContentPickerFieldSettings>();

            foreach (var contentItemId in field.ContentItemIds)
            {
                var contentItem = await _contentManager.GetAsync(contentItemId, VersionOptions.Latest);

                if (contentItem == null)
                {
                    continue;
                }

                var cultureAspect = await _contentManager.PopulateAspectAsync(contentItem, new CultureAspect());

                using (CultureScope.Create(cultureAspect.Culture))
                {
                    model.SelectedItems.Add(new VueMultiselectItemViewModel
                    {
                        Id = contentItemId,
                        DisplayText = await _templateManager.RenderStringAsync(settings.TitlePattern, NullEncoder.Default, contentItem,
                            new Dictionary<string, FluidValue>() { [nameof(ContentItem)] = new ObjectValue(contentItem) }),
                        HasPublished = await _contentManager.HasPublishedVersionAsync(contentItem),
                        IsEditable = await _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext!.User, CommonPermissions.EditContent, contentItem),
                        IsViewable = await _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext!.User, CommonPermissions.ViewContent, contentItem)
                    });
                }

            }
        });
    }

    public override async Task<IDisplayResult> UpdateAsync(ContentPickerField field, UpdateFieldEditorContext context)
    {
        var viewModel = new EditContentPickerFieldViewModel();

        var modelUpdated = await context.Updater.TryUpdateModelAsync(viewModel, Prefix, f => f.ContentItemIds);

        if (!modelUpdated)
        {
            return Edit(field, context);
        }

        field.ContentItemIds = viewModel.ContentItemIds == null
            ? []
            : viewModel.ContentItemIds.Split(',', StringSplitOptions.RemoveEmptyEntries);

        var settings = context.PartFieldDefinition.GetSettings<ContentPickerFieldSettings>();

        if (settings.Required && field.ContentItemIds.Length == 0)
        {
            context.Updater.ModelState.AddModelError(Prefix, nameof(field.ContentItemIds), S["The {0} field is required.", context.PartFieldDefinition.DisplayName()]);
        }

        if (!settings.Multiple && field.ContentItemIds.Length > 1)
        {
            context.Updater.ModelState.AddModelError(Prefix, nameof(field.ContentItemIds), S["The {0} field cannot contain multiple items.", context.PartFieldDefinition.DisplayName()]);
        }

        return Edit(field, context);
    }
}
