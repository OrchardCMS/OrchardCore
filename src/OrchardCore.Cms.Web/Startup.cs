using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OrchardCore.Media;

namespace OrchardCore.Cms.Web
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddOrchardCms().AddSetupFeatures("OrchardCore.AutoSetup");
            services.Configure<IISServerOptions>(options => options.MaxRequestBodySize = 1000000);
            services.Configure<KestrelServerOptions>(options => options.Limits.MaxRequestBodySize = 1000000);
            services.PostConfigure<MediaOptions>(options => options.MaxFileSize = 1000000);
        }

        public void Configure(IApplicationBuilder app, IHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseStaticFiles();

            app.UseOrchardCore();
        }
    }
}
