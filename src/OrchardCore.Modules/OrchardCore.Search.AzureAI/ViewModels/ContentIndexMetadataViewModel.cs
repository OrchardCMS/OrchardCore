using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace OrchardCore.Search.AzureAI.ViewModels;

public class ContentIndexMetadataViewModel
{
    public bool IndexLatest { get; set; }

    public string[] IndexedContentTypes { get; set; }

    public string Culture { get; set; }

    [BindNever]
    public IEnumerable<SelectListItem> Cultures { get; set; }
}
