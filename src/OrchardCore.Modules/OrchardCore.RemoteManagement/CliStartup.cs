using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Modules;

namespace OrchardCore.RemoteManagement;

/// <summary>
/// Enables the Pomi-specific OpenAPI projection and discovery metadata.
/// </summary>
[Feature("OrchardCore.RemoteManagement.Cli")]
public sealed class CliStartup : StartupBase
{
    /// <inheritdoc />
    public override void ConfigureServices(IServiceCollection services)
    {
        services.Configure<RemoteManagementOptions>(options => options.CliEnabled = true);
        services.AddOpenApi(options => options.AddOperationTransformer<CliOperationTransformer>());
    }
}
