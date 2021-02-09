using System;
using System.Collections.Generic;

namespace OrchardCore.Rules
{
    public class ConditionGroup : Condition
    {
        public string Name { get; set; }
        public List<Condition> Conditions { get; set; } = new List<Condition>();
    }   
}