namespace OrchardCore.Navigation;

public class PagerOptions
{
    private int _pageSize = 10;

    public int PageSize
    {
        get
        {
            return _pageSize > MaxPageSize ? MaxPageSize : _pageSize;
        }
        set { _pageSize = value; }
    }

    public int MaxPageSize { get; set; }

}
