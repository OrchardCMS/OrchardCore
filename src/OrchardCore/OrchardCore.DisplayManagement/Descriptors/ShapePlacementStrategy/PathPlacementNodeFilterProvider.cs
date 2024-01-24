using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;

namespace OrchardCore.DisplayManagement.Descriptors.ShapePlacementStrategy
{
    public class PathPlacementNodeFilterProvider : IPlacementNodeFilterProvider
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public PathPlacementNodeFilterProvider(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string Key { get { return "path"; } }

        public bool IsMatch(ShapePlacementContext context, JToken expression)
        {
            IEnumerable<string> paths;

            if (expression is JArray)
            {
                paths = expression.Values<string>();
            }
            else
            {
                paths = new string[] { expression.Value<string>() };
            }

            var requestPath = _httpContextAccessor.HttpContext.Request.Path.Value;

            return paths.Any(p =>
            {
                var normalizedPath = NormalizePath(p);

                if (normalizedPath.EndsWith('*'))
                {
                    var prefix = normalizedPath[..^1];
                    return requestPath.StartsWith(prefix, StringComparison.OrdinalIgnoreCase);
                }

                normalizedPath = AppendTrailingSlash(normalizedPath);
                requestPath = AppendTrailingSlash(requestPath);
                return requestPath.Equals(normalizedPath, StringComparison.OrdinalIgnoreCase);
            });
        }

        private static string NormalizePath(string path)
        {
            if (path.StartsWith("~/", StringComparison.Ordinal))
            {
                return path[1..];
            }
            else if (!path.StartsWith('/'))
            {
                return "/" + path;
            }
            else
            {
                return path;
            }
        }

        private static string AppendTrailingSlash(string path) => path.EndsWith('/') ? path : path + "/";
    }
}
