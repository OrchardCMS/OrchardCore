using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
#if (UseSerilog)
using OrchardCore.Logging;
#endif


namespace OrchardCore.Templates.Cms.Web
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddOrchardCms();
        }
        
        public void Configure(IApplicationBuilder app, IHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseStaticFiles();
#if (UseSerilog)
            app.UseOrchardCore(c => c.UseSerilogTenantNameLogging());
#else
            app.UseOrchardCore();
#endif
        }
    }
}
