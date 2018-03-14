using System;
using System.Collections.Generic;
using Microsoft.Extensions.FileProviders;

namespace OrchardCore.Scripting.Files
{
    public class FilesScriptScope : IScriptingScope
    {
        private static char[] PathSeparators = new[] { '/', '\\' };

        public FilesScriptScope(IFileProvider fileProvider, string basePath)
        {
            FileProvider = fileProvider;
            BasePath = basePath;
        }

        public IFileProvider FileProvider { get; }
        public string BasePath { get; }

        public string GetRelativeFile(string path)
        {
            var baseSegments = BasePath.Split(PathSeparators);
            var pathSegments = path.Split(PathSeparators);

            var segments = new List<string>(baseSegments);

            foreach(var segment in pathSegments)
            {
                if (segment == ".")
                {
                    continue;
                }
                else if (segment == "..")
                {
                    if (segments.Count == 0)
                    {
                        throw new ArgumentException($"Invalid relative path: '{path}'");
                    }

                    segments.RemoveAt(segments.Count - 1);
                }
                else
                {
                    segments.Add(segment);
                }
            }

            return String.Join("/", segments);
        }

    }
}
