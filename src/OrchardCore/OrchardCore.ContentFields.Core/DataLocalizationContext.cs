namespace OrchardCore.ContentFields;

public static class DataLocalizationContext
{
    public const char Separator = ':';

    public static string ContentField(string fieldName)
        => $"Content Fields{Separator}{fieldName}";
}
