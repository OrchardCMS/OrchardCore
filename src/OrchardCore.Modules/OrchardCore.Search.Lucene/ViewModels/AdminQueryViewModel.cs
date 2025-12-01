using System.ComponentModel.DataAnnotations;
using Lucene.Net.Documents;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace OrchardCore.Search.Lucene.ViewModels;

public class AdminQueryViewModel
{
    [Required(AllowEmptyStrings = false, ErrorMessage = "The Query field is required.")]
    public string DecodedQuery { get; set; }

    [Required(AllowEmptyStrings = false, ErrorMessage = "The Index field is required.")]
    public string Id { get; set; }

    public string Parameters { get; set; }

    [BindNever]
    public int Count { get; set; }

    [BindNever]
    public IEnumerable<SelectListItem> Indexes { get; set; }

    [BindNever]
    public string MatchAllQuery { get; internal set; }

    [BindNever]
    public TimeSpan Elapsed { get; set; } = TimeSpan.Zero;

    [BindNever]
    public IEnumerable<Document> Documents { get; set; } = [];
}
