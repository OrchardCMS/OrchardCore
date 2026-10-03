using System.Collections.Specialized;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.Lists.Models;

namespace OrchardCore.Lists.ViewModels;

public class ListPartSettingsViewModel
{
    public NameValueCollection ContentTypes { get; set; }

    public string[] ContainedContentTypes { get; set; }

    public int PageSize { get; set; }

    public bool EnableOrdering { get; set; }

    public bool ShowHeader { get; set; }

    public bool ShowFullPager { get; set; }

    [BindNever]
    public ListPartSettings ListPartSettings { get; set; }
}
