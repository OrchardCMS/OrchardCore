using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Apis.GraphQL;
using OrchardCore.Modules;

namespace OrchardCore.Users.GraphQL;

[RequireFeatures("OrchardCore.Apis.GraphQL", "OrchardCore.Contents")]
public class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<ISchemaBuilder, CurrentUserQuery>();
        services.AddScoped<UserType>();
    }
}
