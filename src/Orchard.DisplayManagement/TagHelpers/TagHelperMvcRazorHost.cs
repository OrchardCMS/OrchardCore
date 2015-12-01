using Microsoft.AspNet.Mvc.Razor;
using Microsoft.AspNet.Mvc.Razor.Directives;
using Microsoft.AspNet.Razor.Runtime.TagHelpers;
using Orchard.DisplayManagement.Descriptors;
using Orchard.DisplayManagement.Theming;

namespace Orchard.DisplayManagement.TagHelpers
{
    public class TagHelperMvcRazorHost : MvcRazorHost
    {
        public TagHelperMvcRazorHost(
            IChunkTreeCache chunkTreeCache,
            IShapeTableManager shapeTableManager,
            IThemeManager themeManager)
            : base(chunkTreeCache)
        {
            TagHelperDescriptorResolver = new ViewComponentTagHelperDescriptorResolver(
                new TagHelperTypeResolver(),
                shapeTableManager,
                themeManager);
        }
    }
}