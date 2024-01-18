using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.Rules.Models;

namespace OrchardCore.Rules.ViewModels
{
    public class JavascriptConditionViewModel
    {
        public string Script { get; set; }

        [BindNever]
        public JavascriptCondition Condition { get; set; }
    }
}
