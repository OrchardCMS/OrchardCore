using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Razor.Compilation.TagHelpers;
using Microsoft.AspNetCore.Razor.Runtime.TagHelpers;
using Microsoft.Extensions.DependencyInjection;
using Orchard.DisplayManagement.Descriptors;

namespace Orchard.DisplayManagement.TagHelpers
{
    public class ShapeTagHelperDescriptorResolver : TagHelperDescriptorResolver, ITagHelperDescriptorResolver
    {
        private static readonly Type ShapeTagHelperType = typeof(ShapeTagHelper);
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ShapeTagHelperDescriptorResolver(
            ITagHelperTypeResolver typeResolver,
            TagHelperDescriptorFactory descriptorFactory,
            IHttpContextAccessor httpContextAccessor
            )
            : base(typeResolver, descriptorFactory)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        IEnumerable<TagHelperDescriptor> ITagHelperDescriptorResolver.Resolve(TagHelperDescriptorResolutionContext resolutionContext)
        {
            // This method is called for every compiled view that contains @tagHelpers. We are caching the shape
            // ones are they are the same for all the views. Furthermore the shapes are discovered using a null
            // theme as we need to grab all the potential shapes

            var descriptors = base.Resolve(resolutionContext);
            var prefix = descriptors.FirstOrDefault()?.Prefix ?? string.Empty;

            var serviceProvider = _httpContextAccessor.HttpContext.RequestServices;
            var shapeTableManager = serviceProvider.GetService<IShapeTableManager>();

            // During Setup, shapeTableManager is null
            if (shapeTableManager == null)
            {
                return descriptors;
            }

            var shapeTagDescriptors = new List<TagHelperDescriptor>();
            foreach (var shape in shapeTableManager.GetShapeTable(null).Descriptors)
            {
                // Don't add the shape tag if another provider already described it
                if (descriptors.Any(x => string.Equals(x.TagName, shape.Key, StringComparison.OrdinalIgnoreCase)))
                {
                    continue;
                }

                shapeTagDescriptors.Add(
                    new TagHelperDescriptor
                    {
                        Prefix = prefix, // The prefix might be different for each call, even if the same shape is rendered
                        TagName = shape.Key,
                        TypeName = ShapeTagHelperType.FullName,
                        AssemblyName = ShapeTagHelperType.GetTypeInfo().Assembly.GetName().Name,
                        Attributes = Enumerable.Empty<TagHelperAttributeDescriptor>(),
                        RequiredAttributes = Enumerable.Empty<TagHelperRequiredAttributeDescriptor>()
                    });
            }

            return descriptors.Concat(shapeTagDescriptors);
        }
    }
}