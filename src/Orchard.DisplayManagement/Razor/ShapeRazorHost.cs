using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Razor.Directives;
using Microsoft.AspNetCore.Razor.Compilation.TagHelpers;
using Microsoft.AspNetCore.Razor.Runtime.TagHelpers;
using Orchard.DisplayManagement.TagHelpers;

namespace Orchard.DisplayManagement.Razor
{
    /// <summary>
    /// Use this class to redefine the list of chunks that a razor page will have
    /// out of the box.
    /// </summary>
    public class ShapeRazorHost : Microsoft.AspNetCore.Mvc.Razor.MvcRazorHost
    {
        /// <summary>
        /// Initializes a new instance of <see cref="MvcRazorHost"/> using the specified <paramref name="chunkTreeCache"/>.
        /// </summary>
        /// <param name="chunkTreeCache">An <see cref="IChunkTreeCache"/> rooted at the application base path.</param>
        /// <param name="resolver">The <see cref="ITagHelperDescriptorResolver"/> used to resolve tag helpers on razor views.</param>
        public ShapeRazorHost(IChunkTreeCache chunkTreeCache, 
            IHttpContextAccessor httpContextAccessor,
            ITagHelperTypeResolver typeResolver,
            ITagHelperDescriptorResolver resolver)
            : base(chunkTreeCache, resolver)
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
