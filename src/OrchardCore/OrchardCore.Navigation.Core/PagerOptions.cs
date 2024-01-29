namespace OrchardCore.Navigation;

public class PagerOptions
{
    private const int DefaultPageSize = 10;

    public int PageSize { get; set; } = DefaultPageSize;

    public int MaxPageSize { get; set; } = 100;

    public int MaxPagedCount { get; set; }

    public int GetPageSize()
    {
        if (MaxPageSize > 0 && PageSize > MaxPageSize)
        {
            return MaxPageSize;
        }

        return PageSize > 0 ? PageSize : DefaultPageSize;
    }
}
