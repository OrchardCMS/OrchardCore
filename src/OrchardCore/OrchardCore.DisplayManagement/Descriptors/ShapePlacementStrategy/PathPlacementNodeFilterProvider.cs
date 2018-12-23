using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OrchardCore.DisplayManagement.Shapes;

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

            var requestPath = _httpContextAccessor.HttpContext.Request.Path;

            return paths.Any(p =>
            {
                var normalizedPath = NormalizePath(p);

                if (normalizedPath.EndsWith("*"))
                {
                    var prefix = normalizedPath.Substring(0, normalizedPath.Length - 1);
                    return requestPath.ToString().StartsWith(prefix, StringComparison.OrdinalIgnoreCase);
                }

                normalizedPath = AppendTrailingSlash(normalizedPath);
                requestPath = AppendTrailingSlash(requestPath);
                return requestPath.ToString().Equals(normalizedPath, StringComparison.OrdinalIgnoreCase);
            });
        }

        private string NormalizePath(string path)
        {
            if (path.StartsWith("~/"))
            {
                return path.Substring(1);
            }
            else if (!path.StartsWith("/"))
            {
                return "/" + path;
            }
            else
            {
                return path;
            }
        }

        private string AppendTrailingSlash(string path)
        {
            return path.EndsWith("/") ? path : path + "/";
        }
    }
}
