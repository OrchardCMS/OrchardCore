namespace OrchardCore.Navigation;

public class PagerOptions
{
    private int _pageSize = 10;
    private int _maxPageSize = 100;

    public int PageSize
    {
        get
        {
            return _pageSize > MaxPageSize ? MaxPageSize : _pageSize;
        }
        set { _pageSize = value; }
    }

    public int MaxPageSize
    {
        get
        {
            return MaxPagedCount > 0 && _maxPageSize > MaxPagedCount ? MaxPagedCount : _maxPageSize;
        }

        set { _maxPageSize = value; }
    }

    public int MaxPagedCount { get; set; }
}
