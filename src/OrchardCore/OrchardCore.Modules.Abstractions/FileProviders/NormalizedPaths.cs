using System;
using System.Collections.Generic;
using System.Linq;

namespace OrchardCore.Modules.FileProviders
{
    public static class NormalizedPaths
    {
        public static void ResolveFolderContents(string folder, IEnumerable<string> normalizedPaths,
            out IEnumerable<string> filePaths, out IEnumerable<string> folderPaths)
        {
            var files = new HashSet<string>(StringComparer.Ordinal);
            var folders = new HashSet<string>(StringComparer.Ordinal);

            if (folder[folder.Length - 1] != '/')
            {
                folder = folder + '/';
            }

            foreach (var path in normalizedPaths.Where(a => a.StartsWith(folder, StringComparison.Ordinal)))
            {
                var folderPath = path.Substring(folder.Length);
                var pathIndex = folderPath.IndexOf('/');
                var isFilePath = pathIndex == -1;

                if (isFilePath)
                {
                    files.Add(path);
                }
                else
                {
                    folders.Add(folderPath.Substring(0, pathIndex));
                }
            }

            filePaths = files;
            folderPaths = folders;
        }
    }
}
