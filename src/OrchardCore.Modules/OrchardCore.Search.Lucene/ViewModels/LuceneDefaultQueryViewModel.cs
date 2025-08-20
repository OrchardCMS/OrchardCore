using Lucene.Net.Util;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace OrchardCore.Search.Lucene.ViewModels;

public class LuceneDefaultQueryViewModel
{
    public string QueryAnalyzerName { get; set; }

    public bool AllowLuceneQueries { get; set; }

    public LuceneVersion DefaultVersion { get; internal set; }

    public SelectListItem[] DefaultSearchFields { get; set; }

    [BindNever]
    public IEnumerable<SelectListItem> Analyzers { get; set; }

    [BindNever]
    public IEnumerable<SelectListItem> DefaultVersions { get; set; }
}
