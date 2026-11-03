using OrchardCore.Localization;

namespace OrchardCore.AzureAI;

/// <summary>
/// Untranslated strings of this module. Their PO context is <c>OrchardCore.AzureAI.AzureAILocalizationSources</c>.
/// </summary>
/// <remarks>
/// Use the sources of <see cref="CommonLocalizationSources"/> for the strings that have the same meaning across modules.
/// </remarks>
internal static class AzureAILocalizationSources
{
    /// <summary>Gets the "Azure AI Search Indexes" text.</summary>
    public static readonly LocalizationSource AzureAISearchIndexes = LocalizationSource.Create("Azure AI Search Indexes", typeof(AzureAILocalizationSources));

    /// <summary>Gets the "Rebuild Azure AI Search Indices" text.</summary>
    public static readonly LocalizationSource RebuildAzureAISearchIndices = LocalizationSource.Create("Rebuild Azure AI Search Indices", typeof(AzureAILocalizationSources));

    /// <summary>Gets the "Reset Azure AI Search Indices" text.</summary>
    public static readonly LocalizationSource ResetAzureAISearchIndices = LocalizationSource.Create("Reset Azure AI Search Indices", typeof(AzureAILocalizationSources));
}
