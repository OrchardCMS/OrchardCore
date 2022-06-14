using Microsoft.AspNetCore.Http;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Builders;
using OrchardCore.Modules;
using System;
using System.Linq;

namespace OrchardCore.Testing.Context
{
    public static class ShellContextExtensions
    {
        public static HttpContext CreateHttpContext(this ShellContext shell)
        {
            var context = shell.Settings.CreateHttpContext();

            context.Features.Set(new ShellContextFeature
            {
                ShellContext = shell,
                OriginalPathBase = String.Empty,
                OriginalPath = "/"
            });

            return context;
        }

        public static HttpContext CreateHttpContext(this ShellSettings settings)
        {
            var context = new DefaultHttpContext().UseShellScopeServices();

            context.Request.Scheme = "https";

            var urlHost = settings.RequestUrlHost?.Split(new[] { ',', ' ' },
                StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();

            context.Request.Host = new HostString(urlHost ?? "localhost");

            if (!String.IsNullOrWhiteSpace(settings.RequestUrlPrefix))
            {
                context.Request.PathBase = "/" + settings.RequestUrlPrefix;
            }

            context.Request.Path = "/";
            context.Items["IsBackground"] = true;

            return context;
        }
    }
}
