using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Apis;
using OrchardCore.Liquid.Fields;
using OrchardCore.Modules;

namespace OrchardCore.Liquid.GraphQL;

[RequireFeatures("OrchardCore.Apis.GraphQL")]
public sealed class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddObjectGraphType<LiquidField, LiquidFieldQueryObjectType>();
    }
}
