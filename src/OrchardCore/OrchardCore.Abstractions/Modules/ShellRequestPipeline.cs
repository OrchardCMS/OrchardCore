using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using OrchardCore.Environment.Shell.Builders;

namespace OrchardCore.Modules
{
    public class ShellRequestPipeline : IShellPipeline
    {
        public IRouter Router { get; set; }
        public RequestDelegate Next { get; set; }
        public Task Invoke(object context) => Next(context as HttpContext);
    }
}
