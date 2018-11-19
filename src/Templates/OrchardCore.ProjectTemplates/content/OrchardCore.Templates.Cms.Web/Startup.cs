using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
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
        
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseStaticFiles();
#if (UseSerilog)
            app.UseOrchardCore(c => c.UseSerilogTenantNameLoggingMiddleware());
#else
            app.UseOrchardCore();
#endif
        }
    }
}
