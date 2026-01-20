using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace OrchardCore.Search.Lucene.ViewModels;

public class LuceneQueryViewModel
{
    public string Index { get; set; }

    public string Query { get; set; }

    public bool ReturnContentItems { get; set; }

    [BindNever]
    public IEnumerable<SelectListItem> Indexes { get; set; }
}
