using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentFields.Settings;
using OrchardCore.ContentFields.ViewModels;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.ContentFields.Fields
{
    public class EnumerationFieldDisplayDriver : ContentFieldDisplayDriver<EnumerationField>
    {
        public EnumerationFieldDisplayDriver(IStringLocalizer<EnumerationFieldDisplayDriver> localizer)
        {
            T = localizer;
        }

        public IStringLocalizer T { get; set; }

        public override IDisplayResult Display(EnumerationField field, BuildFieldDisplayContext context)
        {
            return Initialize<DisplayEnumerationFieldViewModel>("EnumerationField", model =>
            {
                model.Field = field;
                model.Part = context.ContentPart;
                model.PartFieldDefinition = context.PartFieldDefinition;
            })
            .Location("Content")
            .Location("SummaryAdmin", "");
        }

        public override IDisplayResult Edit(EnumerationField field, BuildFieldEditorContext context)
        {
            return Initialize<EditEnumerationFieldViewModel>("EnumerationField_Edit", model =>
            {
                var settings = context.PartFieldDefinition.Settings.ToObject<EnumerationFieldSettings>();
                var options = (!String.IsNullOrWhiteSpace(settings.Options)) ? settings.Options.Split(new string[] { System.Environment.NewLine }, StringSplitOptions.None) : new string[] { T["Select an option"].Value };
                var editorType = settings.Editor.Split('|').Last();


                var optionsSelected = new List<SelectListItem>();
                if (context.IsNew)
                {
                    foreach (var option in options)
                    {
                        var selected = !String.IsNullOrEmpty(settings.DefaultValue) ? settings.DefaultValue.Split(',').Contains(option.Split('|').Last().Trim()) : false;
                        optionsSelected.Add(new SelectListItem { Text = option.Split('|').First().Trim(), Value = option.Split('|').Last().Trim(), Selected = selected });
                    }
                }
                else {
                    foreach (var option in options)
                    {
                        var selected = false;
                        if (editorType == "multi")
                        {
                            selected = field.SelectedValues != null ? field.SelectedValues.Contains(option.Split('|').Last().Trim()) : false;
                        }
                        else if (editorType == "single")
                        {
                            selected = field.Value != null ? field.Value.Contains(option.Split('|').Last().Trim()) : false;
                        }

                        optionsSelected.Add(new SelectListItem { Text = option.Split('|').First().Trim(), Value = option.Split('|').Last().Trim(), Selected = selected });
                    }
                }

                if (editorType == "single")
                {
                    optionsSelected.Insert(0, new SelectListItem { Text = T["Select an option"].Value, Value = "", Selected = false });
                }

                model.Value = field.Value;
                model.Options = optionsSelected;
                model.Field = field;
                model.Part = context.ContentPart;
                model.PartFieldDefinition = context.PartFieldDefinition;
            });
        }

        public override async Task<IDisplayResult> UpdateAsync(EnumerationField field, IUpdateModel updater, UpdateFieldEditorContext context)
        {
            var model = new EditEnumerationFieldViewModel();

            if (await updater.TryUpdateModelAsync(model, Prefix, f => f.Value, f => f.Options))
            {
                var settings = context.PartFieldDefinition.Settings.ToObject<EnumerationFieldSettings>();
                var editorType = settings.Editor.Split('|').Last();

                if (editorType == "single")
                {
                    if (settings.Required && String.IsNullOrEmpty(model.Value))
                    {
                        updater.ModelState.AddModelError(Prefix, T["A value is required for {0}.", context.PartFieldDefinition.DisplayName()]);
                    }
                    else
                    {
                        field.Value = model.Value != "null" ? model.Value : null;
                    }
                }
                else if (editorType == "multi")
                {
                    if (settings.Required && model.Options.Where(x => x.Selected == true).Count() == 0)
                    {
                        updater.ModelState.AddModelError(Prefix, T["A value is required for {0}.", context.PartFieldDefinition.DisplayName()]);
                    }
                    else
                    {
                        field.SelectedValues = model.Options.Where(x => x.Selected == true).Count() > 0 ? model.Options.Where(x => x.Selected == true).Select(s => s.Value).ToArray() : null;
                    }
                }
            }

            return Edit(field, context);
        }
    }
}
