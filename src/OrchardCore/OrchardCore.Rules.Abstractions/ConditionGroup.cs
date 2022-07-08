using System.Collections.Generic;

namespace OrchardCore.Rules
{
    public class ConditionGroup : Condition
    {
        public List<Condition> Conditions { get; set; } = new List<Condition>();
    }

    public class DisplayTextConditionGroup : ConditionGroup
    {
        public string DisplayText { get; set; }
    }
}
