using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Contents.Activities;
using OrchardCore.Modules;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Contents
{
    [RequireFeatures("OrchardCore.Workflows")]
    public class StartupWorkflows : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IActivity, Created>();
            services.AddScoped<IActivity, Deleted>();
            services.AddScoped<IActivity, Published>();
            services.AddScoped<IActivity, Updated>();
            services.AddScoped<IActivity, Versioned>();

            services.AddScoped<IActivity, Delete>();
            services.AddScoped<IActivity, Publish>();
        }
    }
}
