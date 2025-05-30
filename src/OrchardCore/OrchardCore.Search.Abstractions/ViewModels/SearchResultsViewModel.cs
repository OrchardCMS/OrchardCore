using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.ContentManagement;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Search.ViewModels;

public class SearchResultsViewModel : ShapeViewModel
{
    public SearchResultsViewModel()
        : base("Search__Results")
    {
    }

    public SearchResultsViewModel(string shapeType)
        : base(shapeType)
    {
    }

    public string IndexId { get; set; }

    [BindNever]
    public IEnumerable<ContentItem> ContentItems { get; set; }

    [BindNever]
    public Dictionary<string, IReadOnlyDictionary<string, IReadOnlyCollection<string>>> Highlights { get; set; }
}
