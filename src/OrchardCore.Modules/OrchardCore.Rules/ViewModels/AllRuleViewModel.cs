using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.Rules.Models;

namespace OrchardCore.Rules.ViewModels
{
    public class AllRuleViewModel
    {
        public string Name { get; set; }

        [BindNever]
        public AllRule Rule { get; set; }
        
    }
}