using Microsoft.AspNetCore.Http;

namespace OrchardCore
{
    public interface IOrchardHelper
    {
        HttpContext HttpContext { get; }
    }
}
