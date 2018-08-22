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
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.ContentFields.Fields
{
    public class TextFieldDisplayDriver : ContentFieldDisplayDriver<TextField>
    {

        public TextFieldDisplayDriver(IStringLocalizer<TextFieldDisplayDriver> localizer)
        {
            T = localizer;
        }

        public IStringLocalizer T { get; set; }

        public override IDisplayResult Display(TextField field, BuildFieldDisplayContext context)
        {
            return Initialize<DisplayTextFieldViewModel>("TextField", model =>
            {
                model.Field = field;
                model.Part = context.ContentPart;
                model.PartFieldDefinition = context.PartFieldDefinition;
            })
            .Location("Content")
            .Location("SummaryAdmin", "");
        }

        public override IDisplayResult Edit(TextField field, BuildFieldEditorContext context)
        {
            return Initialize<EditTextFieldViewModel>(GetEditorShapeType(context), model =>
            {
                var settings = context.PartFieldDefinition.Settings["TextFieldPredefinedListEditorSettings"].ToObject<TextFieldPredefinedListEditorSettings>();
                var options = (!String.IsNullOrWhiteSpace(settings.Options)) ? settings.Options.Split(new string[] { System.Environment.NewLine }, StringSplitOptions.None) : new string[] { T["Select an option"].Value };

                var optionsSelected = new List<SelectListItem>();
                var optionsGroup = new List<SelectListGroup>();
                if (context.IsNew)
                {
                    foreach (var option in options)
                    {
                        if (option[0] == '#')
                        {
                            optionsGroup.Add(new SelectListGroup { Name = option.Substring(1) });
                        }
                        else
                        {
                            var selected = !String.IsNullOrWhiteSpace(settings.DefaultValue) ? settings.DefaultValue.Split(',').Contains(option.Split('|').Last().Trim()) : false;

                            optionsSelected.Add(new SelectListItem { Text = option.Split('|').First().Trim(), Value = option.Split('|').Last().Trim(), Selected = selected, Group = optionsGroup.Count() > 0 ? optionsGroup.Last() : null });
                        }
                    }
                }
                else
                {
                    foreach (var option in options)
                    {
                        if (option[0] == '#')
                        {
                            optionsGroup.Add(new SelectListGroup { Name = option.Substring(1) });
                        }
                        else
                        {
                            var selected = false;

                            selected = field.Text != null ? field.Text.Contains(option.Split('|').Last().Trim()) : false;
                            optionsSelected.Add(new SelectListItem { Text = option.Split('|').First().Trim(), Value = option.Split('|').Last().Trim(), Selected = selected, Group = optionsGroup.Count() > 0 ? optionsGroup.Last() : null });
                        }
                    }
                }

                //if (editorType == "single")
                //{
                //    optionsSelected.Insert(0, new SelectListItem { Text = T["Select an option"].Value, Value = "", Selected = false });
                //}

                model.Options = optionsSelected;
                model.Text = field.Text;

                model.Field = field;
                model.Part = context.ContentPart;
                model.PartFieldDefinition = context.PartFieldDefinition;
            });
        }

        public override async Task<IDisplayResult> UpdateAsync(TextField field, IUpdateModel updater, UpdateFieldEditorContext context)
        {
            await updater.TryUpdateModelAsync(field, Prefix, f => f.Text);

            return Edit(field, context);
        }
    }
}
