using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.FileProviders;

namespace Orchard.DisplayManagement.FileProviders
{
    public static class FileProviderExtensions
    {
        public static IEnumerable<string> GetViewFilePaths(this IFileProvider fileProvider,
            string subPath, string[] extensions, bool inViewsFolder = false, bool inDepth = true)
        {
            var contents = fileProvider.GetDirectoryContents(subPath);

            if (contents == null)
            {
                yield break;
            }

            if (!inViewsFolder && inDepth)
            {
                var viewsFolder = contents.FirstOrDefault(c => c.Name == "Views" && c.IsDirectory);

                if (viewsFolder != null)
                {
                    foreach (var filePath in GetViewFilePaths(fileProvider, string.Format("{0}/{1}",
                        subPath, viewsFolder.Name), extensions, inViewsFolder: true))
                    {
                        yield return filePath;
                    }

                    yield break;
                }
            }

            foreach (var content in contents)
            {
                if (content.IsDirectory && inDepth)
                {
                    foreach (var filePath in GetViewFilePaths(fileProvider, string.Format("{0}/{1}",
                        subPath, content.Name), extensions, inViewsFolder))
                    {
                        yield return filePath;
                    }
                }
                else if (inViewsFolder)
                {
                    if (inDepth || content.Name.IndexOf("/") == -1)
                    {
                        if (extensions.Contains(Path.GetExtension(content.Name)))
                        {
                            yield return string.Format("{0}/{1}", subPath, content.Name);
                        }
                    }
                }
            }
        }
    }
}
