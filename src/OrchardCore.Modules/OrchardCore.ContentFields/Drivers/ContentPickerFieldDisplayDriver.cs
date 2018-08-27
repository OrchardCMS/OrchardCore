using System;
using System.Threading.Tasks;
using OrchardCore.ContentFields.ViewModels;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.ContentManagement.Records;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using YesSql;
using YesSql.Services;

namespace OrchardCore.ContentFields.Fields
{
    public class ContentPickerFieldDisplayDriver : ContentFieldDisplayDriver<ContentPickerField>
    {
        private readonly ISession _session;

        public ContentPickerFieldDisplayDriver(ISession session)
        {
            _session = session;
        }

        public override IDisplayResult Display(ContentPickerField field, BuildFieldDisplayContext context)
        {
            return Initialize<DisplayContentPickerFieldViewModel>("ContentPickerField", async model =>
            {
                model.Field = field;
                model.Part = context.ContentPart;
                model.PartFieldDefinition = context.PartFieldDefinition;
                model.Updater = context.Updater;
                model.SelectedContentItemIds = string.Join(",", field.ContentItemIds);
                model.ContentItems = await _session.Query<ContentItem, ContentItemIndex>()
                    .With<ContentItemIndex>(x => x.ContentItemId.IsIn(field.ContentItemIds))
                    .ListAsync();
            })
            .Location("Content")
            .Location("SummaryAdmin", "");
        }

        public override IDisplayResult Edit(ContentPickerField field, BuildFieldEditorContext context)
        {
            return Initialize<EditContentPickerFieldViewModel>(GetEditorShapeType(context), model =>
            {
                model.ContentItemIds = string.Join(",", field.ContentItemIds);

                model.Field = field;
                model.Part = context.ContentPart;
                model.PartFieldDefinition = context.PartFieldDefinition;
            });
        }

        public override async Task<IDisplayResult> UpdateAsync(ContentPickerField field, IUpdateModel updater, UpdateFieldEditorContext context)
        {
            var viewModel = new EditContentPickerFieldViewModel();

            await updater.TryUpdateModelAsync(viewModel, Prefix, f => f.ContentItemIds);

            field.ContentItemIds = viewModel.ContentItemIds.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            return Edit(field, context);
        }
    }
}
