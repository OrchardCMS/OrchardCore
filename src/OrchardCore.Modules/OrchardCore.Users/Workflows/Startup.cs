using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Users.Workflows.Activities;
using OrchardCore.Users.Workflows.Drivers;
using OrchardCore.Modules;
using OrchardCore.Workflows.Helpers;
using OrchardCore.Users.Handlers;
using OrchardCore.Users.Workflows.Handlers;

namespace OrchardCore.Users.Workflows
{
    [RequireFeatures("OrchardCore.Workflows")]
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddActivity<RegisterUserTask, RegisterUserTaskDisplay>();
            services.AddActivity<UserCreatedEvent, UserCreatedEventDisplay>();
            services.AddActivity<UserLoggedInEvent, UserLoggedInEventDisplay>();
            services.AddScoped<IUserCreatedEventHandler, UserCreatedHandler>();
            services.AddActivity<AssignUserRoleTask, AssignUserRoleTaskDisplay>();
        }
    }
}
