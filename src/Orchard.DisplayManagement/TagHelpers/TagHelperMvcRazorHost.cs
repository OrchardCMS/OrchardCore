using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Razor.Directives;
using Microsoft.AspNetCore.Razor.Runtime.TagHelpers;

namespace Orchard.DisplayManagement.TagHelpers
{
    public class TagHelperMvcRazorHost : MvcRazorHost
    {
        public TagHelperMvcRazorHost(
            IChunkTreeCache chunkTreeCache,
            ITagHelperTypeResolver typeResolver,
            IHttpContextAccessor httpContextAccessor)
            : base(chunkTreeCache, new TagHelperDescriptorResolver(designTime: false))
        {
            // We need to resolve the services using the scoped service provider
            // explicitly as IRazorViewEngine which is resolving IMvcRazorHost is
            // itself coming from the root service provider.

            // It's fine in this context as the TagHelperMvcRazorHost registration is Transient
            // which means we are not keeping any reference on IShapeTableManager and IThemeManager
            TagHelperDescriptorResolver = new ShapeTagHelperDescriptorResolver(
                typeResolver,
                new TagHelperDescriptorFactory(designTime: false),
                httpContextAccessor
            );
        }
    }
}