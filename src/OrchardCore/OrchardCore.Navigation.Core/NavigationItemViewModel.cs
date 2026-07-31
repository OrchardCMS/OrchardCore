using System.Collections;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement;

namespace OrchardCore.Navigation;

[GenerateShape]
public partial class NavigationItemViewModel : IEnumerable<object>
{
    public NavigationItemViewModel()
    {
        Metadata.Type = "NavigationItem";
    }

    public LocalizedString Text { get; set; }
    public string Href { get; set; }
    public string Target { get; set; }
    public string Url { get; set; }
    public bool LinkToFirstChild { get; set; }
    public RouteValueDictionary RouteValues { get; set; }
    public MenuItem Item { get; set; }
    public IShape Menu { get; set; }
    public IShape Parent { get; set; }
    public int Level { get; set; }
    public int Priority { get; set; }
    public bool Local { get; set; }
    public string Hash { get; set; }
    public int Score { get; set; }
    public bool Selected { get; set; }
    public bool HasItems => Items?.Count > 0;

    public IEnumerator<object> GetEnumerator()
    {
        foreach (var item in Items)
        {
            yield return item;
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
