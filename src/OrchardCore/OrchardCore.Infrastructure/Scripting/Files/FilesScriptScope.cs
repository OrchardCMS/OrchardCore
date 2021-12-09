using Microsoft.Extensions.FileProviders;

namespace OrchardCore.Scripting.Files
{
    public class FilesScriptScope : IScriptingScope
    {
        public FilesScriptScope(IFileProvider fileProvider, string basePath)
        {
            FileProvider = fileProvider;
            BasePath = basePath;
        }

        public IFileProvider FileProvider { get; }
        public string BasePath { get; }
    }
}
