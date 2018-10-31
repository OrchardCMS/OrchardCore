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
    }
}
