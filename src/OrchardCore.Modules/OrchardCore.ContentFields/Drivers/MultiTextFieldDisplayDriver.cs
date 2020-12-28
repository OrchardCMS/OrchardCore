using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;
using OrchardCore.ContentFields.Settings;
using OrchardCore.ContentFields.ViewModels;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Mvc.Utilities;

namespace OrchardCore.ContentFields.Fields
{
    public class MultiTextFieldDisplayDriver : ContentFieldDisplayDriver<MultiTextField>
    {
        private readonly IStringLocalizer S;

        public MultiTextFieldDisplayDriver(IStringLocalizer<MultiTextFieldDisplayDriver> localizer)
        {
            S = localizer;
        }

        public override IDisplayResult Display(MultiTextField field, BuildFieldDisplayContext context)
        {
            return Initialize<DisplayMultiTextFieldViewModel>(GetDisplayShapeType(context), model =>
            {
                model.Field = field;
                model.Part = context.ContentPart;
                model.PartFieldDefinition = context.PartFieldDefinition;
            })
            .Location("Content")
            .Location("SummaryAdmin", "");
        }

        public override IDisplayResult Edit(MultiTextField field, BuildFieldEditorContext context)
        {
            return Initialize<EditMultiTextFieldViewModel>(GetEditorShapeType(context), model =>
            {
                var values = field.Values;
                if (context.IsNew)
                {
                    var settings = context.PartFieldDefinition.GetSettings<MultiTextFieldSettings>();
                    var options = settings.Options.IsJson() ? Array.Empty<MultiTextListValueOption>() : JsonConvert.DeserializeObject<MultiTextListValueOption[]>(settings.Options);

                    values = options.Where(o => o.Default).Select(o => o.Value).ToArray();
                }
                model.Values = values;
                model.Field = field;
                model.Part = context.ContentPart;
                model.PartFieldDefinition = context.PartFieldDefinition;
            });
        }

        public override async Task<IDisplayResult> UpdateAsync(MultiTextField field, IUpdateModel updater, UpdateFieldEditorContext context)
        {
            if (await updater.TryUpdateModelAsync(field, Prefix, f => f.Values))
            {
                var settings = context.PartFieldDefinition.GetSettings<MultiTextFieldSettings>();
                if (settings.Required && (field.Values == null || !field.Values.Any()))
                {
                    updater.ModelState.AddModelError(Prefix, S["A value is required for {0}.", context.PartFieldDefinition.DisplayName()]);
                }
            }

            return Edit(field, context);
        }
    }
}
