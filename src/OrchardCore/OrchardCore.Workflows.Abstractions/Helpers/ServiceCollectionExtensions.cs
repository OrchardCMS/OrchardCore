using Microsoft.Extensions.DependencyInjection;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Workflows.Helpers
{
    public static class ServiceCollectionExtensions
    {
        public static void AddActivity<TActivity, TDriver>(this IServiceCollection services) where TActivity : class, IActivity where TDriver : class, IDisplayDriver<IActivity>
        {
            services.AddScoped<IActivity, TActivity>();
            services.AddScoped<IDisplayDriver<IActivity>, TDriver>();
        }
    }
}
