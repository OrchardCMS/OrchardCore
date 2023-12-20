using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace OrchardCore.Search.Azure.CognitiveSearch.ViewModels;

public class CognativeSearchSettingsViewModel
{
    public string IndexName { get; set; }

    public string AnalyzerName { get; set; }

    public bool IndexLatest { get; set; }

    public string Culture { get; set; }

    public string[] IndexedContentTypes { get; set; }

    public bool IsCreate { get; set; }

    [BindNever]
    public IEnumerable<SelectListItem> Analyzers { get; set; }

    [BindNever]
    public IEnumerable<SelectListItem> Cultures { get; set; }
}
