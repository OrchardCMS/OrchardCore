using System;
using OrchardCore.Filters.Abstractions.Nodes;

namespace OrchardCore.Filters.Abstractions.Services
{
    public abstract class TermOption
    {
        public TermOption(string name)
        {
            Name = name;
        }

        public string Name { get; }

        /// <summary>
        /// Whether one or many of the specified term is allowed.
        /// </summary>
        public bool Single { get; set; } = true;

        public Delegate MapTo { get; set; }
        public Delegate MapFrom { get; set; }
        public Func<string, string, TermNode> MapFromFactory { get; set; }
    }
}
