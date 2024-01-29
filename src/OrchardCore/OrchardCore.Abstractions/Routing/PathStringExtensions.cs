using System;
using Microsoft.AspNetCore.Http;

namespace OrchardCore.Routing
{
    public static class PathStringExtensions
    {
        public static bool StartsWithNormalizedSegments(this PathString path, PathString other)
        {
            if (other.HasValue && other.Value.EndsWith('/'))
            {
                return path.StartsWithSegments(other.Value[..^1]);
            }

            return path.StartsWithSegments(other);
        }

        public static bool StartsWithNormalizedSegments(this PathString path, PathString other, StringComparison comparisonType)
        {
            if (other.HasValue && other.Value.EndsWith('/'))
            {
                return path.StartsWithSegments(other.Value[..^1], comparisonType);
            }

            return path.StartsWithSegments(other, comparisonType);
        }

        public static bool StartsWithNormalizedSegments(this PathString path, PathString other, out PathString remaining)
        {
            if (other.HasValue && other.Value.EndsWith('/'))
            {
                return path.StartsWithSegments(other.Value[..^1], out remaining);
            }

            return path.StartsWithSegments(other, out remaining);
        }

        public static bool StartsWithNormalizedSegments(this PathString path, PathString other, StringComparison comparisonType, out PathString remaining)
        {
            if (other.HasValue && other.Value.EndsWith('/'))
            {
                return path.StartsWithSegments(other.Value[..^1], comparisonType, out remaining);
            }

            return path.StartsWithSegments(other, comparisonType, out remaining);
        }
    }
}
