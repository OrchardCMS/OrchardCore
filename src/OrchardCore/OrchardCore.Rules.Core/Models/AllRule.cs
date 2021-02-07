using System;

namespace OrchardCore.Rules.Models
{
    public class GroupRule : Rule
    {
        public Rule[] Children { get; set; } = Array.Empty<Rule>();
    }

    public class AllRule : GroupRule
    {
    }    
}