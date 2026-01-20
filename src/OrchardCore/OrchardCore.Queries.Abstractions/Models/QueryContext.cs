namespace OrchardCore.Queries;

public sealed class QueryContext
{
    public string Source { get; set; }

    public string Name { get; set; }

    public bool Sorted { get; set; }
}
