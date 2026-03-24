namespace OrchardCore.Localization.Data;

public static class DataLocalizationContext
{
    public const char Separator = ':';
    public const string ContentTypes = "ContentTypes";

    public static string ContentFields(string fieldName)
        => $"Content Fields{Separator}{fieldName}";

    public static string Permission(string groupName = null) => groupName is null
        ? "Permissions"
        : $"Permissions{Separator}{groupName}";
}
