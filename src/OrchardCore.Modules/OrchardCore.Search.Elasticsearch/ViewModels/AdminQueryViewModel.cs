using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace OrchardCore.Search.Elasticsearch.ViewModels;

public class AdminQueryViewModel
{
    [Required(AllowEmptyStrings = false, ErrorMessage = "The Query field is required.")]
    public string DecodedQuery { get; set; }

    [Required(AllowEmptyStrings = false, ErrorMessage = "The Index field is required.")]
    public string Id { get; set; }

    public string Parameters { get; set; }

    [BindNever]
    public long Count { get; set; }

    [BindNever]
    public IEnumerable<SelectListItem> Indexes { get; set; }

    [BindNever]
    public TimeSpan Elapsed { get; set; } = TimeSpan.Zero;

    [BindNever]
    public IEnumerable<ElasticsearchRecord> Documents { get; set; } = [];

    [BindNever]
    public IEnumerable<ElasticsearchRecord> Fields { get; set; } = [];

    [BindNever]
    public string MatchAllQuery { get; set; }
}
