using System;
using OrchardCore.ContentManagement;

namespace OrchardCore.Sitemaps
{
    public class SitemapsRazorPagesContentTypeOption
    {
        public SitemapsRazorPagesContentTypeOption(string contentType)
        {
            ContentType = contentType;
        }

        /// <summary>
        /// The content type name using razor pages.
        /// </summary>
        public string ContentType { get; }

        /// <summary>
        /// The razor page name.
        /// </summary>
        public string PageName { get; set; }

        /// <summary>
        /// Route values used to create the link to the razor page.
        /// Must include the area, or namespace of module, the razor page belongs to. 
        /// </summary>
        public Func<ContentItem, object> RouteValues { get; set; }
    }
}
