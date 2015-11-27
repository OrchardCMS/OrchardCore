using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNet.Razor.Runtime.TagHelpers;
using Microsoft.AspNet.Razor.TagHelpers;
using Microsoft.AspNet.Razor.Compilation.TagHelpers;
using Orchard.DisplayManagement.Descriptors;
using Orchard.DependencyInjection;

namespace Orchard.DisplayManagement.TagHelpers
{
    public class ViewComponentTagHelperDescriptorResolver : TagHelperDescriptorResolver, ITagHelperDescriptorResolver, ISingletonDependency
    {
        private static readonly Type ShapeTagHelperType = typeof(ShapeTagHelper);
        private readonly IShapeTableManager _shapteTableManager;

        public ViewComponentTagHelperDescriptorResolver(
            TagHelperTypeResolver typeResolver,
            IShapeTableManager shapeTableManager)
            : base(typeResolver, designTime: false)
        {
            _shapteTableManager = shapeTableManager;
        }

        IEnumerable<TagHelperDescriptor> ITagHelperDescriptorResolver.Resolve(TagHelperDescriptorResolutionContext resolutionContext)
        {
            var descriptors = base.Resolve(resolutionContext);

            var shapeTagDescriptors = ResolveViewComponentTagHelperDescriptors(descriptors.FirstOrDefault()?.Prefix ?? string.Empty);

            // Remove the descriptors that were hravested from tooling classes
            var allShapeTags = shapeTagDescriptors
                .Select(x => x.TagName.ToLowerInvariant())
                .ToArray();

            descriptors = descriptors
                .Where(x => !allShapeTags.Contains(x.TagName.ToLowerInvariant()));

            return descriptors.Concat(shapeTagDescriptors);
        }

        private IEnumerable<TagHelperDescriptor> ResolveViewComponentTagHelperDescriptors(string prefix)
        {
            var resolvedDescriptors = new List<TagHelperDescriptor>();

            var attributeDescriptors = new List<TagHelperAttributeDescriptor>();

            foreach (var shape in _shapteTableManager.GetShapeTable(null).Descriptors)
            {
                resolvedDescriptors.Add(
                    new TagHelperDescriptor
                    {
                        Prefix = "",
                        TagName = shape.Key,
                        TypeName = ShapeTagHelperType.FullName,
                        AssemblyName = ShapeTagHelperType.GetTypeInfo().Assembly.GetName().Name,
                        Attributes = Enumerable.Empty<TagHelperAttributeDescriptor>(),
                        RequiredAttributes = Enumerable.Empty<string>(),
                    });
            }

            return resolvedDescriptors;
        }
    }
}