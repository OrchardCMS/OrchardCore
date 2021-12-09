using Microsoft.AspNetCore.Http;

namespace OrchardCore.Modules.FileProviders
{
    public interface IVirtualPathBaseProvider
    {
        PathString VirtualPathBase { get; }
    }
}
