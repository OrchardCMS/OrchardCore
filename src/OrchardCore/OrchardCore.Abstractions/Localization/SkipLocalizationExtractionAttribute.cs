namespace OrchardCore.Localization;

/// <summary>
/// Excludes localization calls within the marked class or method from build-time extraction.
/// Calls to marked methods from unmarked code are still extracted.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = false)]
public sealed class SkipLocalizationExtractionAttribute : Attribute
{
}
