using System;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace OrchardCore.Tests.Apis
{
    public class SiteStartup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddBareOrchard(null, Path.Combine("sites", "tests"));
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            app.UseModules();
        }
    }
}
