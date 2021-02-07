using System;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.Rules.Models;

namespace OrchardCore.Rules.ViewModels
{
    public class RuleGroupViewModel
    {
        public RuleEntry[] Entries { get; set; }

        [BindNever]
        public RuleGroup Rule { get; set; }
        
    }

    public class RuleEntry
    {
        [BindNever]
        public Rule Rule { get; set; }
    }
}