using System;
using System.Collections.Generic;
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
using OrchardCore.Mvc.ModelBinding;
using OrchardCore.Mvc.Utilities;

namespace OrchardCore.ContentFields.Fields
{
    public class KeyValuePairsFieldDisplayDriver : ContentFieldDisplayDriver<KeyValuePairsField>
    {
        private readonly IStringLocalizer S;

        public KeyValuePairsFieldDisplayDriver(IStringLocalizer<KeyValuePairsFieldDisplayDriver> localizer)
        {
            S = localizer;
        }

        public override IDisplayResult Display(KeyValuePairsField field, BuildFieldDisplayContext context)
        {
            return Initialize<DisplayKeyValuePairsFieldViewModel>(GetDisplayShapeType(context), model =>
            {
                var settings = context.PartFieldDefinition.GetSettings<KeyValuePairsFieldSettings>();

                model.Field = field;
                model.Part = context.ContentPart;
                model.PartFieldDefinition = context.PartFieldDefinition;
            })
            .Location("Detail", "Content")
            .Location("Summary", "Content");
        }

        public override IDisplayResult Edit(KeyValuePairsField field, BuildFieldEditorContext context)
        {
            return Initialize<EditKeyValuePairsFieldViewModel>(GetEditorShapeType(context), model =>
            {
                if (context.IsNew)
                {
                    var settings = context.PartFieldDefinition.GetSettings<KeyValuePairsFieldSettings>();
                    model.Values = JsonConvert.SerializeObject(Array.Empty<string>());
                    model.Keys = JsonConvert.SerializeObject(Array.Empty<string>());
                }
                else
                {
                    model.Keys = JsonConvert.SerializeObject(field.Values.Select(x => x.Key).ToArray());
                    model.Values = JsonConvert.SerializeObject(field.Values.Select(x => x.Value).ToArray());
                }

                model.Field = field;
                model.Part = context.ContentPart;
                model.PartFieldDefinition = context.PartFieldDefinition;
            });
        }

        public override async Task<IDisplayResult> UpdateAsync(KeyValuePairsField field, IUpdateModel updater, UpdateFieldEditorContext context)
        {
            var viewModel = new EditKeyValuePairsFieldViewModel();
            if (await updater.TryUpdateModelAsync(viewModel, Prefix))
            {
                var keys = JsonConvert.DeserializeObject<string[]>(viewModel.Keys);
                var values = JsonConvert.DeserializeObject<string[]>(viewModel.Values);

                field.Values = keys.Zip(values).Select(x => new KeyValuePair<string, string>(x.First, x.Second)).ToArray();

                var settings = context.PartFieldDefinition.GetSettings<KeyValuePairsFieldSettings>();
                if (settings.Required && !viewModel.Values.Any())
                {
                    updater.ModelState.AddModelError(Prefix, nameof(field.Values), S["A value is required for {0}.", context.PartFieldDefinition.DisplayName()]);
                }
            }

            return Edit(field, context);
        }
    }
}
