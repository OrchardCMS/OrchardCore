using OrchardCore.Localization;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Secrets;

public static class SecretsPermissions
{
    public static readonly Permission ManageSecrets = 
        new("ManageSecrets", LocalizationSource.Create("Manage secrets", typeof(SecretsPermissions)));

    public static readonly Permission ViewSecrets = 
        new("ViewSecrets", LocalizationSource.Create("View secrets", typeof(SecretsPermissions)), [ManageSecrets]);
}
