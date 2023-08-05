using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.FileProviders;

namespace OrchardCore.Modules.FileProviders
{
    public static class FileInfoExtensions
    {
        public static IEnumerable<string> ReadAllLines(this IFileInfo fileInfo)
        {
            var lines = new List<string>();

            if (fileInfo?.Exists ?? false)
            {
                using var reader = fileInfo.CreateReadStream();
                using var sr = new StreamReader(reader);

                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    lines.Add(line);
                }
            }

            return lines;
        }
    }
}
