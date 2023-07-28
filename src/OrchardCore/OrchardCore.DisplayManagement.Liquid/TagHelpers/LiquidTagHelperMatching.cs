using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Razor.Language;

namespace OrchardCore.DisplayManagement.Liquid.TagHelpers
{
    public class LiquidTagHelperMatching
    {
        private const string AspPrefix = "asp-";
        public readonly static LiquidTagHelperMatching None = new();
        public readonly TagMatchingRuleDescriptor[] _rules = Array.Empty<TagMatchingRuleDescriptor>();

        public LiquidTagHelperMatching()
        {
        }

        public LiquidTagHelperMatching(string name, string assemblyName, IEnumerable<TagMatchingRuleDescriptor> tagMatchingRules)
        {
            Name = name;
            AssemblyName = assemblyName;
            _rules = tagMatchingRules.ToArray();
        }

        public string Name { get; } = String.Empty;
        public string AssemblyName { get; } = String.Empty;

        private static bool Predicate(TagMatchingRuleDescriptor rule, string helper, IEnumerable<string> arguments)
        {
            // Does it match the required tag name
            if (rule.TagName != "*" && !String.Equals(rule.TagName, helper, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            // Does it expect any specific attribute?
            if (!rule.Attributes.Any())
            {
                return true;
            }

            // Are all required attributes present?
            var allRequired = rule.Attributes.All(attr => arguments.Any(name =>
            {
                // Exact match
                if (String.Equals(name, attr.Name, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }

                // Check by replacing all '_' with '-', e.g. asp_src will map to asp-src
                name = name.Replace('_', '-');

                if (String.Equals(name, attr.Name, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }

                if (attr.Name.StartsWith(AspPrefix, StringComparison.Ordinal))
                {
                    if (name.AsSpan().Equals(attr.Name.AsSpan(AspPrefix.Length), StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }

                return false;
            }));

            if (allRequired)
            {
                return true;
            }

            return false;
        }

        public bool Match(string helper, IEnumerable<string> arguments)
        {
            foreach (var rule in _rules)
            {
                if (Predicate(rule, helper, arguments))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
