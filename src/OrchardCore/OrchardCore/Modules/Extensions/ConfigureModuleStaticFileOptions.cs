using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Modules;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Configures Module Static File Providers and Set Cache
    /// </summary>
    internal class ConfigureModuleStaticFileOptions : IPostConfigureOptions<StaticFileOptions>
    {
        private readonly IModuleStaticFileProvider _fileProvider;
        private readonly IShellConfiguration _shellConfiguration;
        private readonly IWebHostEnvironment _environment;
        public ConfigureModuleStaticFileOptions(IModuleStaticFileProvider fileProvider,
            IShellConfiguration shellConfiguration,
            IWebHostEnvironment environment)
        {
            _fileProvider = fileProvider;
            _shellConfiguration = shellConfiguration;
            _environment = environment;
        }
        public void PostConfigure(string name, StaticFileOptions options)
        {
            name = name ?? throw new ArgumentNullException(nameof(name));
            options = options ?? throw new ArgumentNullException(nameof(options));

            if (name != Microsoft.Extensions.Options.Options.DefaultName)
            {
                return;
            }

            var serveWebRoot = _shellConfiguration.GetValue("StaticFileOptions:ServeHostWebRoot", false);
            if (serveWebRoot)
            {
                // Serve Tenant Static files from module and from HostWebRoot
                options.FileProvider = new CompositeFileProvider(_fileProvider, _environment.WebRootFileProvider);
            }
            else
            {
                // Serve Tenant Static files only from module
                options.FileProvider = _fileProvider;
            }

            var cacheControl = _shellConfiguration.GetValue("StaticFileOptions:CacheControl", $"public, max-age={TimeSpan.FromDays(30).TotalSeconds}, s-max-age={TimeSpan.FromDays(365.25).TotalSeconds}");
            var beforePrepare = options.OnPrepareResponse;

            options.OnPrepareResponse = ctx => OnPrepareResponseCacheControl(ctx, cacheControl, beforePrepare);
        }

        private static void OnPrepareResponseCacheControl(StaticFileResponseContext ctx, string cacheControl, Action<StaticFileResponseContext> beforePrepare)
        {
            if (beforePrepare != null)
            {
                beforePrepare(ctx);
            }

            // Cache static files for a year as they are coming from embedded resources and should not vary
            ctx.Context.Response.Headers[HeaderNames.CacheControl] = cacheControl;
        }
    }
}
