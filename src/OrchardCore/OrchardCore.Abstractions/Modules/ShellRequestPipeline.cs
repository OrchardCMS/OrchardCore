using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace OrchardCore.Modules
{
    public class ShellRequestPipeline
    {
        public RequestDelegate Next;
        public IRouter Router;
    }
}
