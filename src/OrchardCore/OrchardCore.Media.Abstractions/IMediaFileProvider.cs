using OrchardCore.Abstractions.Modules.FileProviders;
using OrchardCore.Modules.FileProviders;

namespace OrchardCore.Media
{
    public interface IMediaFileProvider : IStaticFileProvider, IVirtualPathBaseProvider
    {
    }
}
