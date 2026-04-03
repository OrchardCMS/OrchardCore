namespace OrchardCore.Localization.Data;

public static class DataLocalizationContext
{
    public const char Separator = ':';
    public const string ContentType = "Content Types";

    public static string ContentField(string fieldName)
        => $"Content Fields{Separator}{fieldName}";

    public static string Permission(string groupName = null) => groupName is null
        ? "Permissions"
        : $"Permissions{Separator}{groupName}";
}
