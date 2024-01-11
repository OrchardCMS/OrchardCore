using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.Rules.Models;

namespace OrchardCore.Rules.ViewModels
{
    public class AllConditionViewModel
    {
        public string DisplayText { get; set; }

        [BindNever]
        public AllConditionGroup Condition { get; set; }
    }
}
