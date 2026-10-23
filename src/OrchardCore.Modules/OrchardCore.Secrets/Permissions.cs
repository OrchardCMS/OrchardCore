using OrchardCore.Security.Permissions;

namespace OrchardCore.Secrets;

public static class SecretsPermissions
{
    public static readonly Permission ManageSecrets = new("ManageSecrets", "Manage secrets");

    public static readonly Permission ViewSecrets = new("ViewSecrets", "View secrets", [ManageSecrets]);
}
