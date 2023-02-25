using System.ComponentModel.DataAnnotations;

namespace OrchardCore.Search.ViewModel;

public class SearchPartViewModel
{
    [Required]
    public string Placeholder { get; set; }

    public string IndexName { get; set; }
}
