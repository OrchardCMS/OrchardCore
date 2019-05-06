using Microsoft.Extensions.FileProviders;

namespace OrchardCore.Modules
{
    /// <summary>
    /// This custom <see cref="IFileProvider"/> implementation provides Di registration identification
    /// for IFileProviders that should be served via UseStaticFiles.
    /// </summary>
    public interface IModuleStaticFileProvider : IFileProvider
    {
    }
}
