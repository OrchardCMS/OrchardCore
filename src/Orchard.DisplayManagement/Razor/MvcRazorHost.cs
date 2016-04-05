using Microsoft.AspNetCore.Mvc.Razor.Directives;
using Microsoft.AspNetCore.Razor.Compilation.TagHelpers;

namespace Orchard.DisplayManagement.Razor
{
    /// <summary>
    /// Use this class to redefine the list of chunks that a razor page will have
    /// out of the box.
    /// </summary>
    public class MvcRazorHost : Microsoft.AspNetCore.Mvc.Razor.MvcRazorHost
    {
        /// <summary>
        /// Initializes a new instance of <see cref="MvcRazorHost"/> using the specified <paramref name="chunkTreeCache"/>.
        /// </summary>
        /// <param name="chunkTreeCache">An <see cref="IChunkTreeCache"/> rooted at the application base path.</param>
        /// <param name="resolver">The <see cref="ITagHelperDescriptorResolver"/> used to resolve tag helpers on razor views.</param>
        public MvcRazorHost(IChunkTreeCache chunkTreeCache, ITagHelperDescriptorResolver resolver)
            : base(chunkTreeCache, resolver)
        {
        }
    }
}
