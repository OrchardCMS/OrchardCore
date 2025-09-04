using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.Rules.Models;

namespace OrchardCore.Rules.ViewModels;

public class BooleanConditionViewModel
{
    public bool Value { get; set; }

    [BindNever]
    public BooleanCondition Condition { get; set; }
}
