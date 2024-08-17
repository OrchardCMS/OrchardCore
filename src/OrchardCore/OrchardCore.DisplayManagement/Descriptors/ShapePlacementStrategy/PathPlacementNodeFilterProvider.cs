using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Http;

namespace OrchardCore.DisplayManagement.Descriptors.ShapePlacementStrategy;

public class PathPlacementNodeFilterProvider : IPlacementNodeFilterProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public PathPlacementNodeFilterProvider(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string Key { get { return "path"; } }

    public bool IsMatch(ShapePlacementContext context, object expression)
    {
        IEnumerable<string> paths;

        var jsonNode = JNode.FromObject(expression);
        if (jsonNode is JsonArray jsonArray)
        {
            paths = jsonArray.Values<string>();
        }
        else
        {
            paths = new string[] { jsonNode.Value<string>() };
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
