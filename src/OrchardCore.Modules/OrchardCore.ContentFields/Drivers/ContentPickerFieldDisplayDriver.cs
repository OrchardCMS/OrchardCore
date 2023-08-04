using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fluid;
using Fluid.Values;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentFields.Settings;
using OrchardCore.ContentFields.ViewModels;
using OrchardCore.ContentLocalization;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentManagement.Models;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Liquid;
using OrchardCore.Localization;
using OrchardCore.Mvc.ModelBinding;

namespace OrchardCore.ContentFields.Drivers
{
    public class ContentPickerFieldDisplayDriver : ContentFieldDisplayDriver<ContentPickerField>
    {
        private readonly IContentManager _contentManager;
        private readonly ILiquidTemplateManager _templateManager;
        protected readonly IStringLocalizer S;

        public ContentPickerFieldDisplayDriver(
            IContentManager contentManager,
            IStringLocalizer<ContentPickerFieldDisplayDriver> localizer,
            ILiquidTemplateManager templateManager)
        {
            _contentManager = contentManager;
            S = localizer;
            _templateManager = templateManager;
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
                model.ContentItemIds = String.Join(",", field.ContentItemIds);

                model.Field = field;
                model.Part = context.ContentPart;
                model.PartFieldDefinition = context.PartFieldDefinition;

                model.SelectedItems = new List<VueMultiselectItemViewModel>();
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
                            HasPublished = await _contentManager.HasPublishedVersionAsync(contentItem)
                        });
                    }
                }
            });
        }

        public override async Task<IDisplayResult> UpdateAsync(ContentPickerField field, IUpdateModel updater, UpdateFieldEditorContext context)
        {
            var viewModel = new EditContentPickerFieldViewModel();

            var modelUpdated = await updater.TryUpdateModelAsync(viewModel, Prefix, f => f.ContentItemIds);

            if (!modelUpdated)
            {
                return Edit(field, context);
            }

            field.ContentItemIds = viewModel.ContentItemIds == null
                ? Array.Empty<string>() : viewModel.ContentItemIds.Split(',', StringSplitOptions.RemoveEmptyEntries);

            var settings = context.PartFieldDefinition.GetSettings<ContentPickerFieldSettings>();

            if (settings.Required && field.ContentItemIds.Length == 0)
            {
                updater.ModelState.AddModelError(Prefix, nameof(field.ContentItemIds), S["The {0} field is required.", context.PartFieldDefinition.DisplayName()]);
            }

            if (!settings.Multiple && field.ContentItemIds.Length > 1)
            {
                updater.ModelState.AddModelError(Prefix, nameof(field.ContentItemIds), S["The {0} field cannot contain multiple items.", context.PartFieldDefinition.DisplayName()]);
            }

            return Edit(field, context);
        }
    }
}
