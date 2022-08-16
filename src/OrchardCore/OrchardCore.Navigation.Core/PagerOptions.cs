using System.ComponentModel;

namespace OrchardCore.Navigation;

public class PagerOptions
{
    [DefaultValue(10)]
    public int PageSize { get; set; } = 10;

    [DefaultValue(100)]
    public int MaxPageSize { get; set; } = 100;

    public int MaxPagedCount { get; set; }
}
