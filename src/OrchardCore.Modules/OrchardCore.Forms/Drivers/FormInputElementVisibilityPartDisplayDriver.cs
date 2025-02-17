using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Forms.Models;
using OrchardCore.Forms.ViewModels;

namespace OrchardCore.Forms.Drivers;

internal sealed class FormInputElementVisibilityPartDisplayDriver : ContentPartDisplayDriver<FormInputElementVisibilityPart>
{
    internal readonly IStringLocalizer S;

    public FormInputElementVisibilityPartDisplayDriver(IStringLocalizer<FormInputElementVisibilityPartDisplayDriver> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public override IDisplayResult Edit(FormInputElementVisibilityPart part, BuildPartEditorContext context)
    {
        return Initialize<FormInputElementVisibilityViewModel>("FormInputElementVisibility_Edit", model =>
        {
            model.Actions = new List<SelectListItem>
            {
                new SelectListItem(S["None"], nameof(FormVisibilityAction.None)),
                new SelectListItem(S["Show"], nameof(FormVisibilityAction.Show)),
                new SelectListItem(S["Hide"], nameof(FormVisibilityAction.Hide)),
            };

            model.Groups = new List<FormVisibilityRuleGroupViewModel>
            {
                new FormVisibilityRuleGroupViewModel
                {
                    Rules = new List<FormVisibilityRuleViewModel>
                    {
                        new FormVisibilityRuleViewModel
                        {
                            Fields = new List<FormVisibilityFieldViewModel>
                            {
                                new FormVisibilityFieldViewModel
                                {
                                    Name = "Field 1",
                                    Value = "Field1",
                                },
                                new FormVisibilityFieldViewModel
                                {
                                    Name = "Field 2",
                                    Value = "Field2",
                                },
                            },
                            Operators = new List<SelectListItem>
                            {
                                new SelectListItem(S["Is"], nameof(FormVisibilityOperator.Is)),
                                new SelectListItem(S["Is not"], nameof(FormVisibilityOperator.IsNot)),
                                new SelectListItem(S["Empty"], nameof(FormVisibilityOperator.Empty)),
                                new SelectListItem(S["Not empty"],nameof(FormVisibilityOperator.NotEmpty)),
                                new SelectListItem(S["Contains"], nameof(FormVisibilityOperator.Contains)),
                                new SelectListItem(S["Does not contain"], nameof(FormVisibilityOperator.DoesNotContain)),
                                new SelectListItem(S["Starts with"], nameof(FormVisibilityOperator.StartsWith)),
                                new SelectListItem(S["Ends with"], nameof(FormVisibilityOperator.EndsWith)),
                                new SelectListItem(S["Greater then"], nameof(FormVisibilityOperator.GreaterThan)),
                                new SelectListItem(S["Less then"], nameof(FormVisibilityOperator.LessThan)),
                            },
                        },
                    },
                },
            };
        }).Location("Parts:0#Visibility Settings;5");
    }

    public override async Task<IDisplayResult> UpdateAsync(FormInputElementVisibilityPart part, UpdatePartEditorContext context)
    {
        var model = new FormInputElementVisibilityViewModel();

        await context.Updater.TryUpdateModelAsync(model, Prefix);

        part.Groups = model.Groups.Select(x => new FormVisibilityRuleGroup
        {
            Rules = x.Rules.Select(y => new FormVisibilityRule
            {
                Field = y.Field,
                Operator = y.Operator,
                Values = GetValues(y.Value),
            }).ToList(),
        });

        return Edit(part, context);
    }

    private static string[] GetValues(string value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            try
            {
                return JsonSerializer.Deserialize<string[]>(value);
            }
            catch { }
        }

        return [];
    }
}
