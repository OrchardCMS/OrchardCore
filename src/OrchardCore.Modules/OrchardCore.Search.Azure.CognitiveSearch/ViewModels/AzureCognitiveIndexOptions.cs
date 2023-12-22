using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace OrchardCore.Search.Azure.CognitiveSearch.ViewModels;

public class AzureCognitiveIndexOptions
{
    public AzureCognitiveIndexBulkAction BulkAction { get; set; }

    public string Search { get; set; }

    [BindNever]
    public List<SelectListItem> ContentsBulkAction { get; set; }
}
