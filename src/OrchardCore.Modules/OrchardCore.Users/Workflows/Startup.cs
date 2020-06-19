using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Modules;
using OrchardCore.Users.Handlers;
using OrchardCore.Users.Workflows.Activities;
using OrchardCore.Users.Workflows.Drivers;
using OrchardCore.Users.Workflows.Handlers;
using OrchardCore.Workflows.Helpers;

namespace OrchardCore.Users.Workflows
{
    [RequireFeatures("OrchardCore.Workflows")]
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddActivity<RegisterUserTask, RegisterUserTaskDisplay>();
            services.AddActivity<UserCreatedEvent, UserCreatedEventDisplay>();
            services.AddActivity<UserEnabledEvent, UserEnabledEventDisplay>();
            services.AddActivity<UserDisabledEvent, UserDisabledEventDisplay>();
            services.AddActivity<UserLoggedInEvent, UserLoggedInEventDisplay>();
            services.AddScoped<IUserEventHandler, UserEventHandler>();
            services.AddActivity<AssignUserRoleTask, AssignUserRoleTaskDisplay>();
        }
    }
}
