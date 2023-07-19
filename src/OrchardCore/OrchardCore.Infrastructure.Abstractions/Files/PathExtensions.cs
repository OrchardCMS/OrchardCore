using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Primitives;
using System;

namespace System.IO
{
    public class PathExtensions
    {
        public static readonly char[] PathSeparators = new[] { '/', '\\' };
        private const string CurrentDirectoryToken = ".";
        private const string ParentDirectoryToken = "..";

        /// <summary>
        /// Combines two path parts
        /// </summary>
        public static string Combine(string path, string other = null)
        {
            if (String.IsNullOrWhiteSpace(other))
            {
                return path;
            }

            if (other.StartsWith('/') || other.StartsWith('\\'))
            {
                // "other" is already an app-rooted path. Return it as-is.
                return other;
            }

            string result;

            var index = path.LastIndexOfAny(PathSeparators);

            if (index != path.Length - 1)
            {
                // If the first ends in a trailing slash e.g. "/Home/", assume it's a directory.
                result = path + "/" + other;
            }
            else
            {
                result = String.Concat(path.AsSpan(0, index + 1), other);
            }

            return result;
        }

        /// <summary>
        /// Combines multiple path parts
        /// </summary>
        public static string Combine(string path, params string[] others)
        {
            string result = path;

            for (var i = 0; i < others.Length; i++)
            {
                result = Combine(result, others[i]);
            }

            return result;
        }

        /// <summary>
        /// Resolves relative segments in a path
        /// </summary>
        public static string ResolvePath(string path)
        {
            var pathSegment = new StringSegment(path);
            if (path[0] == PathSeparators[0] || path[0] == PathSeparators[1])
            {
                // Leading slashes (e.g. "/Views/Index.cshtml") always generate an empty first token. Ignore these
                // for purposes of resolution.
                pathSegment = pathSegment.Subsegment(1);
            }

            var tokenizer = new StringTokenizer(pathSegment, PathSeparators);
            var requiresResolution = false;
            foreach (var segment in tokenizer)
            {
                // Determine if we need to do any path resolution.
                // We need to resolve paths with multiple path separators (e.g "//" or "\\") or, directory traversals e.g. ("../" or "./").
                if (segment.Length == 0 ||
                    segment.Equals(ParentDirectoryToken) ||
                    segment.Equals(CurrentDirectoryToken))
                {
                    requiresResolution = true;
                    break;
                }
            }

            if (!requiresResolution)
            {
                return path;
            }

            var pathSegments = new List<StringSegment>();
            foreach (var segment in tokenizer)
            {
                if (segment.Length == 0)
                {
                    // Ignore multiple directory separators
                    continue;
                }
                if (segment.Equals(ParentDirectoryToken))
                {
                    if (pathSegments.Count == 0)
                    {
                        // Don't resolve the path if we ever escape the file system root. We can't reason about it in a
                        // consistent way.
                        return path;
                    }
                    pathSegments.RemoveAt(pathSegments.Count - 1);
                }
                else if (segment.Equals(CurrentDirectoryToken))
                {
                    // We already have the current directory
                    continue;
                }
                else
                {
                    pathSegments.Add(segment);
                }
            }

            var builder = new StringBuilder();
            for (var i = 0; i < pathSegments.Count; i++)
            {
                var segment = pathSegments[i];
                builder.Append('/');
                builder.Append(segment.Buffer, segment.Offset, segment.Length);
            }

            return builder.ToString();
        }
    }
}
