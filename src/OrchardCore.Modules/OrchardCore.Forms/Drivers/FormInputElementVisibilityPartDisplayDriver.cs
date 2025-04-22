using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Forms.Models;
using OrchardCore.Forms.ViewModels;

namespace OrchardCore.Forms.Drivers;

internal sealed class FormInputElementVisibilityPartDisplayDriver : ContentPartDisplayDriver<FormInputElementVisibilityPart>
{
    internal readonly IStringLocalizer S;

    public FormInputElementVisibilityPartDisplayDriver(
        IStringLocalizer<FormInputElementVisibilityPartDisplayDriver> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public override IDisplayResult Display(FormInputElementVisibilityPart part, BuildPartDisplayContext context)
    {
        return Initialize<FormInputElementVisibilityDisplayViewModel>("FormInputElementVisibilityPartVisibility", model =>
        {
            if (part.ContentItem.TryGet<FormInputElementVisibilityPart>(out var visibilityPart) &&
            visibilityPart.Action != FormVisibilityAction.None)
            {
                var name = part.ContentItem.As<FormInputElementPart>()?.Name;

                model.ElementName = name;
                model.Action = visibilityPart.Action;

                model.Groups = (visibilityPart.Groups ?? []).Select(group =>
                new FormVisibilityRuleGroupDisplayViewModel
                {
                    Rules = (group.Rules.Select(rule =>
                        new FormVisibilityRuleDisplayViewModel
                        {
                            Field = rule.Field,
                            Operator = rule.Operator.ToString(),
                            Value = rule.Values?.FirstOrDefault()
                        }
                    ))
                });
            }
        }).Location("Detail", "Content");
    }

    public override IDisplayResult Edit(FormInputElementVisibilityPart part, BuildPartEditorContext context)
    {
        return Initialize<FormInputElementVisibilityViewModel>("FormInputElementVisibility_Edit", model =>
        {
            var operators = new List<SelectListItem>()
            {
                new(S["Is"], nameof(FormVisibilityOperator.Is)),
                new(S["Is not"], nameof(FormVisibilityOperator.IsNot)),
                new(S["Empty"], nameof(FormVisibilityOperator.Empty)),
                new(S["Not empty"], nameof(FormVisibilityOperator.NotEmpty)),
                new(S["Contains"], nameof(FormVisibilityOperator.Contains)),
                new(S["Does not contain"], nameof(FormVisibilityOperator.DoesNotContain)),
                new(S["Starts with"], nameof(FormVisibilityOperator.StartsWith)),
                new(S["Ends with"], nameof(FormVisibilityOperator.EndsWith)),
                new(S["Greater than"], nameof(FormVisibilityOperator.GreaterThan)),
                new(S["Less than"], nameof(FormVisibilityOperator.LessThan)),
            };

            model.Operators = operators;
            model.Action = part.Action;
            model.Actions =
            [
                new(S["Always visible"], nameof(FormVisibilityAction.None)),
                new(S["Conditionally show"], nameof(FormVisibilityAction.Show)),
                new(S["Conditionally hide"], nameof(FormVisibilityAction.Hide)),
            ];

            model.Groups = (part.Groups ?? [])
                .Select(group => new FormVisibilityRuleGroupViewModel
                {
                    Rules = (group.Rules ?? [])
                    .Select(rule => new FormVisibilityRuleViewModel
                    {
                        Field = rule.Field,
                        Operator = rule.Operator.ToString() ?? string.Empty,
                        Value = rule.Values?.FirstOrDefault() ?? string.Empty,
                        Fields = [],
                        Operators = operators,
                    })
                });
        }).Location("Parts:0#Visibility Settings;5");
    }

    public override async Task<IDisplayResult> UpdateAsync(FormInputElementVisibilityPart part, UpdatePartEditorContext context)
    {
        var model = new FormInputElementVisibilityViewModel();

        await context.Updater.TryUpdateModelAsync(model, Prefix);

        part.Action = model.Action;

        part.Groups = (model.Groups ?? Enumerable.Empty<FormVisibilityRuleGroupViewModel>()).Select(group =>
        {
            return new FormVisibilityRuleGroup
            {
                Rules = group.Rules?.Select(rule =>
                {
                    var parsedOperator = FormVisibilityOperator.Is;
                    if (!string.IsNullOrWhiteSpace(rule.Operator))
                    {
                        parsedOperator = Enum.Parse<FormVisibilityOperator>(rule.Operator);
                    }

                    return new FormVisibilityRule
                    {
                        Field = rule.Field,
                        Operator = parsedOperator,
                        Values = GetValues(rule.Value),
                    };
                })
            };
        });

        return Edit(part, context);
    }

    private static string[] GetValues(string value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            try
            {
                var jsonArray = new[]
                {
                    value
                };
                return jsonArray;
            }
            catch { }
        }

        return [];
    }
}
