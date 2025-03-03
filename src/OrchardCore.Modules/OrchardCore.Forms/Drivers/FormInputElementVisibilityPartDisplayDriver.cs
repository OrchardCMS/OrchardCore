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
        part.Groups ??= part.Groups ?? new List<FormVisibilityRuleGroup>();
        if (!part.Groups.Any())
        {
            part.Groups = new List<FormVisibilityRuleGroup>
            {
               new FormVisibilityRuleGroup
               {
                  Rules = new List<FormVisibilityRule>
                  {
                    new FormVisibilityRule
                    {
                      Field = "",
                      Operator = FormVisibilityOperator.Is,
                      Values = Array.Empty<string>()
                    }
                  }
               }
            };
        }

        return Initialize<FormInputElementVisibilityViewModel>("FormInputElementVisibility_Edit", model =>
        {
            model.Action = part.Action;
            model.Actions = new List<SelectListItem>
            {
              new SelectListItem(S["None"], nameof(FormVisibilityAction.None)),
              new SelectListItem(S["Show"], nameof(FormVisibilityAction.Show)),
              new SelectListItem(S["Hide"], nameof(FormVisibilityAction.Hide)),
            };

            model.Groups = part.Groups.Select(group =>
            {
                return new FormVisibilityRuleGroupViewModel
                {
                    Rules = group.Rules.Select(rule =>
                    {
                        return new FormVisibilityRuleViewModel
                        {
                            Field = rule.Field,
                            Operator = rule.Operator,
                            Value = rule.Values?.FirstOrDefault() ?? string.Empty,
                            TargetInputId = rule.TargetInputId,
                            Fields = new List<FormVisibilityFieldViewModel>(),
                            Operators = new List<SelectListItem>
                            {
                               new SelectListItem(S["Is"], nameof(FormVisibilityOperator.Is)),
                               new SelectListItem(S["Is not"], nameof(FormVisibilityOperator.IsNot)),
                               new SelectListItem(S["Empty"], nameof(FormVisibilityOperator.Empty)),
                               new SelectListItem(S["Not empty"], nameof(FormVisibilityOperator.NotEmpty)),
                               new SelectListItem(S["Contains"], nameof(FormVisibilityOperator.Contains)),
                               new SelectListItem(S["Does not contain"], nameof(FormVisibilityOperator.DoesNotContain)),
                               new SelectListItem(S["Starts with"], nameof(FormVisibilityOperator.StartsWith)),
                               new SelectListItem(S["Ends with"], nameof(FormVisibilityOperator.EndsWith)),
                               new SelectListItem(S["Greater than"], nameof(FormVisibilityOperator.GreaterThan)),
                               new SelectListItem(S["Less than"], nameof(FormVisibilityOperator.LessThan)),
                            }
                        };
                    }).ToList()
                };
            }).ToList();
        }).Location("Parts:0#Visibility Settings;5");
    }
    public override async Task<IDisplayResult> UpdateAsync(FormInputElementVisibilityPart part, UpdatePartEditorContext context)
    {
        var model = new FormInputElementVisibilityViewModel();

        await context.Updater.TryUpdateModelAsync(model, Prefix);
        Console.WriteLine("Updated model: Action = " + model.Action);
        part.Action = model.Action;

        part.Groups = (model.Groups ?? new List<FormVisibilityRuleGroupViewModel>())
            .Select(x =>
            {
                return new FormVisibilityRuleGroup
                {
                    Rules = x.Rules.Select(y =>
                    {
                        Console.WriteLine($"Updating rule: Field = {y.Field}, Value = {y.Value}");
                        return new FormVisibilityRule
                        {
                            Field = y.Field,
                            Operator = y.Operator,
                            Values = GetValues(y.Value),
                            TargetInputId = y.TargetInputId
                        };
                    }).ToList()
                };
            }).ToList();

        return Edit(part, context);
    }

    private static string[] GetValues(string value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            try
            {
                var jsonArray = new[] { value };
                return jsonArray;
            }
            catch { }
        }
        return Array.Empty<string>();
    }
}
