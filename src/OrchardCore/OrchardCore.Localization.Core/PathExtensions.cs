using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Primitives;

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

            if (other.StartsWith("/", StringComparison.Ordinal) || other.StartsWith("\\", StringComparison.Ordinal))
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
                result = path.Substring(0, index + 1) + other;
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
    }
}
