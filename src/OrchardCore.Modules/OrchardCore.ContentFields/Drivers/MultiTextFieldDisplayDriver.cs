using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentFields.Settings;
using OrchardCore.ContentFields.ViewModels;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Mvc.ModelBinding;
using OrchardCore.Mvc.Utilities;

namespace OrchardCore.ContentFields.Fields
{
    public class MultiTextFieldDisplayDriver : ContentFieldDisplayDriver<MultiTextField>
    {
        protected readonly IStringLocalizer S;

        public MultiTextFieldDisplayDriver(IStringLocalizer<MultiTextFieldDisplayDriver> localizer)
        {
            S = localizer;
        }

        public override IDisplayResult Display(MultiTextField field, BuildFieldDisplayContext context)
        {
            return Initialize<DisplayMultiTextFieldViewModel>(GetDisplayShapeType(context), model =>
            {
                var settings = context.PartFieldDefinition.GetSettings<MultiTextFieldSettings>();

                model.Values = settings.Options.Where(o => field.Values?.Contains(o.Value) == true).Select(o => o.Value).ToArray();
                model.Field = field;
                model.Part = context.ContentPart;
                model.PartFieldDefinition = context.PartFieldDefinition;
            })
            .Location("Detail", "Content")
            .Location("Summary", "Content");
        }

        public override IDisplayResult Edit(MultiTextField field, BuildFieldEditorContext context)
        {
            return Initialize<EditMultiTextFieldViewModel>(GetEditorShapeType(context), model =>
            {
                if (context.IsNew)
                {
                    var settings = context.PartFieldDefinition.GetSettings<MultiTextFieldSettings>();
                    model.Values = settings.Options.Where(o => o.Default).Select(o => o.Value).ToArray();
                }
                else
                {
                    model.Values = field.Values;
                }
                model.Field = field;
                model.Part = context.ContentPart;
                model.PartFieldDefinition = context.PartFieldDefinition;
            });
        }

        public override async Task<IDisplayResult> UpdateAsync(MultiTextField field, IUpdateModel updater, UpdateFieldEditorContext context)
        {
            var viewModel = new EditMultiTextFieldViewModel();
            if (await updater.TryUpdateModelAsync(viewModel, Prefix))
            {
                field.Values = viewModel.Values;

                var settings = context.PartFieldDefinition.GetSettings<MultiTextFieldSettings>();
                if (settings.Required && !viewModel.Values.Any())
                {
                    updater.ModelState.AddModelError(Prefix, nameof(field.Values), S["A value is required for {0}.", context.PartFieldDefinition.DisplayName()]);
                }
            }

            return Edit(field, context);
        }
    }
}
