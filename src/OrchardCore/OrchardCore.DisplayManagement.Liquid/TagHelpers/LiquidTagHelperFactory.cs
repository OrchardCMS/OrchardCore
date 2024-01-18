using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Fluid;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Razor.TagHelpers;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace OrchardCore.DisplayManagement.Liquid.TagHelpers
{
    /// <summary>
    /// Singleton containing shared state for tag helper tags.
    /// </summary>
    public class LiquidTagHelperFactory
    {
        private static readonly ConcurrentDictionary<Type, LiquidTagHelperMatching> _allMatchings = new();
        private static readonly ConcurrentDictionary<Type, LiquidTagHelperActivator> _allActivators = new();

        private List<LiquidTagHelperMatching> _matchings;
        private readonly ApplicationPartManager _partManager;
        private readonly ITagHelperFactory _factory;

        public LiquidTagHelperFactory(ApplicationPartManager partManager, ITagHelperFactory factory)
        {
            _partManager = partManager;
            _factory = factory;
        }

        private void EnsureMatchings()
        {
            if (_matchings != null)
            {
                return;
            }

            lock (this)
            {
                if (_matchings == null)
                {
                    var feature = new TagHelperFeature();
                    _partManager.PopulateFeature(feature);

                    var matchings = new List<LiquidTagHelperMatching>();

                    foreach (var tagHelper in feature.TagHelpers)
                    {
                        var matching = _allMatchings.GetOrAdd(tagHelper.AsType(), type =>
                        {
                            var descriptorBuilder = TagHelperDescriptorBuilder.Create(
                                type.FullName, type.Assembly.GetName().Name);

                            descriptorBuilder.SetTypeName(type.FullName);
                            AddTagMatchingRules(type, descriptorBuilder);
                            var descriptor = descriptorBuilder.Build();

                            return new LiquidTagHelperMatching(
                                descriptor.Name,
                                descriptor.AssemblyName,
                                descriptor.TagMatchingRules
                            );
                        });

                        matchings.Add(matching);
                    }

                    _matchings = matchings;
                }
            }
        }

        public LiquidTagHelperActivator GetActivator(string helper, IEnumerable<string> arguments)
        {
            EnsureMatchings();
            var matching = _matchings.Where(d => d.Match(helper, arguments)).FirstOrDefault() ?? LiquidTagHelperMatching.None;

            if (matching != LiquidTagHelperMatching.None)
            {
                var tagHelperType = Type.GetType(matching.Name + ", " + matching.AssemblyName);
                return _allActivators.GetOrAdd(tagHelperType, type => new LiquidTagHelperActivator(type));
            }

            return LiquidTagHelperActivator.None;
        }

        public ITagHelper CreateTagHelper(LiquidTagHelperActivator activator, ViewContext context, FilterArguments arguments,
            out TagHelperAttributeList contextAttributes, out TagHelperAttributeList outputAttributes)
        {
            return activator.Create(_factory, context, arguments, out contextAttributes, out outputAttributes);
        }

        private static void AddTagMatchingRules(Type type, TagHelperDescriptorBuilder descriptorBuilder)
        {
            var targetElementAttributes = type.GetCustomAttributes<HtmlTargetElementAttribute>();

            // If there isn't an attribute specifying the tag name derive it from the name
            if (!targetElementAttributes.Any())
            {
                var name = type.Name;

                if (name.EndsWith("TagHelper", StringComparison.OrdinalIgnoreCase))
                {
                    name = name[..^"TagHelper".Length];
                }

                descriptorBuilder.TagMatchingRule(ruleBuilder =>
                {
                    var htmlCasedName = HtmlConventions.ToHtmlCase(name);
                    ruleBuilder.TagName = htmlCasedName;
                });

                return;
            }

            foreach (var targetElementAttribute in targetElementAttributes)
            {
                descriptorBuilder.TagMatchingRule(ruleBuilder =>
                {
                    var tagName = targetElementAttribute.Tag;
                    ruleBuilder.TagName = tagName;

                    var parentTag = targetElementAttribute.ParentTag;
                    ruleBuilder.ParentTag = parentTag;

                    var tagStructure = targetElementAttribute.TagStructure;
                    ruleBuilder.TagStructure = (Microsoft.AspNetCore.Razor.Language.TagStructure)tagStructure;

                    var requiredAttributeString = targetElementAttribute.Attributes;
                    RequiredAttributeParser.AddRequiredAttributes(requiredAttributeString, ruleBuilder);
                });
            }
        }
    }
}
