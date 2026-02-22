namespace OrchardCore.Localization.Data;

internal sealed class NullDataTranslationProvider : IDataTranslationProvider
{
    public ValueTask LoadTranslationsAsync(string cultureName, CultureDictionary dictionary) => ValueTask.CompletedTask;
}
