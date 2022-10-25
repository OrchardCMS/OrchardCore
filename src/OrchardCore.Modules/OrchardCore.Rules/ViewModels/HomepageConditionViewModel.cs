using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.Rules.Models;

namespace OrchardCore.Rules.ViewModels
{
    public class HomepageConditionViewModel
    {
        public bool Value { get; set; }

        [BindNever]
        public HomepageCondition Condition { get; set; }
    }
}
