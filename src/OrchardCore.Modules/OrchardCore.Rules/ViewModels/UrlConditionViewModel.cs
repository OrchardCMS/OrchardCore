using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.Rules.Models;

namespace OrchardCore.Rules.ViewModels
{
    public class UrlConditionViewModel
    {
        public string SelectedOperation { get; set; }
        public string Value { get; set; }

        [BindNever]
        public UrlCondition Condition { get; set; }
    }
}
