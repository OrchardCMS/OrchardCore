using System;
using System.Collections.Generic;
using System.Linq;

namespace OrchardCore.Environment.Commands
{
    [AttributeUsage(AttributeTargets.Method)]
    public class OrchardSwitchesAttribute : Attribute
    {
        private readonly string _switches;

        public OrchardSwitchesAttribute(string switches)
        {
            _switches = switches;
        }

        public IEnumerable<string> Switches
        {
            get
            {
                return (_switches ?? "").Trim().Split(',').Select(s => s.Trim());
            }
        }
    }
}
