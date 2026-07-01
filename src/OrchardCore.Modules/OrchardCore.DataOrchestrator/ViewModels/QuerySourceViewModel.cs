using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace OrchardCore.DataOrchestrator.ViewModels;

public class QuerySourceViewModel
{
    [Required]
    public string QueryName { get; set; }

    public string ParametersJson { get; set; }

    public IList<SelectListItem> AvailableQueries { get; set; } = [];
}
