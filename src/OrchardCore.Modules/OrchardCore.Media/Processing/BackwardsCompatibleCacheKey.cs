using Microsoft.AspNetCore.Http;
using SixLabors.ImageSharp.Web;
using SixLabors.ImageSharp.Web.Caching;
using SixLabors.ImageSharp.Web.Commands;

namespace OrchardCore.Media.Processing
{
    /// <summary>
    /// Backwards compatible absolute url cache key.
    /// </summary>
    public class BackwardsCompatibleCacheKey : ICacheKey
    {
        /// <inheritdoc/>
        public string Create(HttpContext context, CommandCollection commands)
        {

            var pathBase = context.Request.PathBase;
            if (pathBase.HasValue)
            {
                // Due to bugs with an earlier version of cache calculation the cache key result was
                // localhost:44300/agency1//media/portfolio/5.jpg?width=600&height=480&rmode=stretch
                // the default ImageSharp absolute cache builder produces the correct value
                // localhost:44300/agency1/media/portfolio/5.jpg?width=600&height=480&rmode=stretch
                // which causes an entire cache refresh.
                // refer https://github.com/SixLabors/ImageSharp.Web/issues/254

                pathBase = new PathString(pathBase + "//");
            }

            return CaseHandlingUriBuilder.BuildAbsolute(CaseHandlingUriBuilder.CaseHandling.LowerInvariant, context.Request.Host, pathBase, context.Request.Path, QueryString.Create(commands));
        }
    }
}
