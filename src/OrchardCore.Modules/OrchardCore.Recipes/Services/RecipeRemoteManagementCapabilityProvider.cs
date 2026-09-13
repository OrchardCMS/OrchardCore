using OrchardCore.RemoteManagement;

namespace OrchardCore.Recipes.Services;

internal sealed class RecipeRemoteManagementCapabilityProvider : IRemoteManagementCapabilityProvider
{
    public ValueTask<IEnumerable<RemoteManagementCapability>> GetCapabilitiesAsync() =>
        ValueTask.FromResult<IEnumerable<RemoteManagementCapability>>(
        [
            new RemoteManagementCapability
            {
                Id = "recipes",
                Version = "1.0",
                DisplayName = "Recipes",
            },
        ]);
}
