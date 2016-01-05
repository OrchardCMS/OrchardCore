using Microsoft.AspNet.Http;
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
            IHttpContextAccessor httpContextAccessor)
            : base(chunkTreeCache)
        {
            // We need to resolve the services using the scoped service provider
            // explicitly as IRazorViewEngine which is resolving IMvcRazorHost is
            // itself coming from the root service provider.
            var httpContext = httpContextAccessor.HttpContext;

            // It's fine in this context as the TagHelperMvcRazorHost registration is Transient
            // which means we are not keeping any reference on IShapeTableManager and IThemeManager
            TagHelperDescriptorResolver = new ViewComponentTagHelperDescriptorResolver(
                new TagHelperTypeResolver(),
                (IShapeTableManager)httpContext.RequestServices.GetService(typeof(IShapeTableManager)),
                (IThemeManager)httpContext.RequestServices.GetService(typeof(IThemeManager))
            );
        }
    }
}