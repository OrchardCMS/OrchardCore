using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Razor.Language;

namespace OrchardCore.DisplayManagement.Liquid.Tags.TagHelpers
{
    public class TagHelperMatching
    {
        private const string AspPrefix = "asp-";
        public readonly static TagHelperMatching None = new TagHelperMatching();
        public readonly IEnumerable<TagMatchingRuleDescriptor> _rules;

        public TagHelperMatching() { }

        public TagHelperMatching(string name, string assemblyName, IEnumerable<TagMatchingRuleDescriptor> tagMatchingRules)
        {
            Name = name;
            AssemblyName = assemblyName;
            _rules = tagMatchingRules.ToArray();
        }

        public string Name { get; } = String.Empty;
        public string AssemblyName { get; } = String.Empty;

        public bool Match(string helper, IEnumerable<string> arguments)
        {
            return _rules.Any(rule => ((rule.TagName == "*") || rule.TagName == helper) &&
                (!rule.Attributes.Any() || rule.Attributes.All(attr => arguments.Any(name =>
                {
                    if (String.Equals(name, attr.Name, StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }

                    name = name.Replace('_', '-');

                    if (attr.Name.StartsWith(AspPrefix) && String.Equals(name,
                        attr.Name.Substring(AspPrefix.Length), StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }

                    if (String.Equals(name, attr.Name, StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }

                    return false;
                }))));
        }
    }
}