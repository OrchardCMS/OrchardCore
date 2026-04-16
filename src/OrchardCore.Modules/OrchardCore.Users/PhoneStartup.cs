using Microsoft.Extensions.DependencyInjection;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Modules;
using OrchardCore.Users.Drivers;
using OrchardCore.Users.Models;

namespace OrchardCore.Users;

[Feature(UserConstants.Features.Phone)]
public sealed class PhoneStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddDisplayDriver<User, UserPhoneNumberDisplayDriver>();
    }
}
