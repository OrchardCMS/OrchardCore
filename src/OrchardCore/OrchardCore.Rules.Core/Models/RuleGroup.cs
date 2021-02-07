using System;
using System.Collections.Generic;

namespace OrchardCore.Rules.Models
{
    public class RuleGroup : Rule
    {
        public string Name { get; set; }
        public List<Rule> Rules { get; set; } = new List<Rule>();
    }   
}