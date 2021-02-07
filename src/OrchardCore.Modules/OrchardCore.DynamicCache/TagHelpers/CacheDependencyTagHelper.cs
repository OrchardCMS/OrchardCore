using System;
using Microsoft.AspNetCore.Razor.TagHelpers;
using OrchardCore.Environment.Cache;

namespace OrchardCore.DynamicCache.TagHelpers
{
    [HtmlTargetElement("cache-dependency", Attributes = DependencyAttributeName)]
    public class CacheDependencyTagHelper : TagHelper
    {
        private const string DependencyAttributeName = "dependency";

        /// <summary>
        /// Gets or sets a <see cref="string" /> with the dependency to invalidate the cache with.
        /// </summary>
        [HtmlAttributeName(DependencyAttributeName)]
        public string Dependency { get; set; }

        private readonly ICacheScopeManager _cacheScopeManager;

        public CacheDependencyTagHelper(
            ICacheScopeManager cacheScopeManager)

        {
            _cacheScopeManager = cacheScopeManager;
        }


        /// <inheritdoc />
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (output == null)
            {
                throw new ArgumentNullException(nameof(output));
            }

            if (!String.IsNullOrEmpty(Dependency))
            {
                _cacheScopeManager.AddDependencies(Dependency);
            }

            // Clear the contents of the "cache-dependency" element since we don't want to render it.
            output.SuppressOutput();
        }
    }
}
