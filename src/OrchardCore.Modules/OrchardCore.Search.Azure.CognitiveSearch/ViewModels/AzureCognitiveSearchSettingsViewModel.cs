using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace OrchardCore.Search.Azure.CognitiveSearch.ViewModels;

public class AzureCognitiveSearchSettingsViewModel
{
    public string SearchFields { get; set; }

    public string SearchIndex { get; set; }

    [BindNever]
    public IList<SelectListItem> SearchIndexes { get; set; }
}
