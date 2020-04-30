using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OrchardCore.MiniProfiler;

namespace OrchardCore.Cms.Web
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddOrchardCms(builder => builder.ConfigureServices(services =>
                services.PostConfigure<MiniProfilerOptions>(settings =>
                {
                    settings.EnableOnAdmin = true;
                })));
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
