namespace OrchardCore.Search.Models;

public class IndexRecipeStep<T>
    where T : IndexSettingsBase
{
    public string Name { get; set; }

    public IList<Dictionary<string, T>> Indexes { get; set; } = [];

    // Some search providers like Elasticserach and Lucene use the term "Indices" for the plural of index, but the
    // common spelling in US English is "Indexes". So both are supported to avoid unnecessary spelling problems.
    [Obsolete($"Maintained for backwards compatibility, please use {nameof(Indexes)} instead.")]
    public IList<Dictionary<string, T>> Indices
    {
        get => Indexes;
        set => Indexes = value?.Count > 0 ? value : Indexes;
    }
}
