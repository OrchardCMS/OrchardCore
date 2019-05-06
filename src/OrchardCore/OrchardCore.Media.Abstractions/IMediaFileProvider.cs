using Microsoft.Extensions.FileProviders;
using OrchardCore.Abstractions.Modules.FileProviders;

namespace OrchardCore.Media
{
    public interface IMediaFileProvider : IFileProvider, IVirtualPathBaseProvider
    {
    }
}
