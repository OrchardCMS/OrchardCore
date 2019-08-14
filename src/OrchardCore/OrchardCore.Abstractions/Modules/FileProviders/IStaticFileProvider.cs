using Microsoft.Extensions.FileProviders;

namespace OrchardCore.Modules.FileProviders
{
    /// <summary>
    /// This custom <see cref="IFileProvider"/> implementation provides Di registration identification
    /// for IStaticFileProviders that should be served via UseStaticFiles.
    /// </summary>
    public interface IStaticFileProvider : IFileProvider
    {
    }
}
