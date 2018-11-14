using System.Threading.Tasks;
using Fluid;
using Microsoft.AspNetCore.Hosting;

namespace OrchardCore.Liquid.Services
{
    public class HostingEnvironmentLiquidTemplateEventHandler : ILiquidTemplateEventHandler
    {
        private readonly IHostingEnvironment _hostingEnvironment;

        public HostingEnvironmentLiquidTemplateEventHandler(IHostingEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
        }

        public Task RenderingAsync(TemplateContext context)
        {
            var hostingEnvironment = new HostingEnvironment(_hostingEnvironment);
            context.LocalScope.SetValue("HostingEnvironment", hostingEnvironment);
            context.MemberAccessStrategy.Register(hostingEnvironment.GetType());
            return Task.CompletedTask;
        }

        private class HostingEnvironment
        {
            private readonly IHostingEnvironment _hostingEnvironment;

            public HostingEnvironment(IHostingEnvironment hostingEnvironment)
            {
                _hostingEnvironment = hostingEnvironment;
            }

            public string ApplicationName => _hostingEnvironment.ApplicationName;

            public string ContentRootPath => _hostingEnvironment.ContentRootPath;

            public string EnvironmentName => _hostingEnvironment.EnvironmentName;

            public string WebRootPath => _hostingEnvironment.WebRootPath;

            public bool IsDevelopment => _hostingEnvironment.IsDevelopment();

            public bool IsProduction => _hostingEnvironment.IsProduction();

            public bool IsStaging => _hostingEnvironment.IsStaging();
        }
    }
}