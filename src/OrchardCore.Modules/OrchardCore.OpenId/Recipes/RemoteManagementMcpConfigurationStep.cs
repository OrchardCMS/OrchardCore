using System.Text.Json.Nodes;
using OrchardCore.OpenId.ViewModels;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;

namespace OrchardCore.OpenId.Recipes;

/// <summary>
/// Configures an MCP client using explicit clientId and redirectUris recipe values.
/// </summary>
public sealed class RemoteManagementMcpConfigurationStep : NamedRecipeStepHandler
{
    private readonly RemoteManagementMcpConfigurationService _configurationService;

    public RemoteManagementMcpConfigurationStep(RemoteManagementMcpConfigurationService configurationService)
        : base("RemoteManagementMcpConfiguration")
    {
        _configurationService = configurationService;
    }

    protected override Task HandleAsync(RecipeExecutionContext context) =>
        _configurationService.ConfigureAsync(context.Step.ToObject<RemoteManagementMcpViewModel>());
}
