using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.Rules.Models;

namespace OrchardCore.Rules.ViewModels
{
    public class AnyConditionViewModel
    {
        public string DisplayText { get; set; }

        [BindNever]
        public AnyConditionGroup Condition { get; set; }
    }
}
