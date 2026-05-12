namespace OrchardCore.Roles;

public static class DataLocalizationContext
{
    public static string Permission(string groupName = null) => groupName is null
        ? "Permissions"
        : $"Permissions{OrchardCoreConstants.DataLocalizationSeparator}{groupName}";
}
