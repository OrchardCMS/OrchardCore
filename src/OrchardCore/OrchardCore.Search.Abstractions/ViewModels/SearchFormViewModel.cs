using Microsoft.AspNetCore.Mvc;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Search.Abstractions.ViewModels
{
    [BindProperties(SupportsGet = true)]
    public class SearchFormViewModel : ShapeViewModel
    {
        public SearchFormViewModel(string shapeType) : base(shapeType) { }

        public string Terms { get; set; }

        public string IndexName { get; set; } = "Search";
    }
}
