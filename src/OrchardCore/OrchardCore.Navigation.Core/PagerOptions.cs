namespace OrchardCore.Navigation;

public class PagerOptions
{
    public int PageSize { get; set; } = 10;

    public int MaxPageSize { get; set; } = 100;

    public int MaxPagedCount { get; set; }
}
