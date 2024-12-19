using Microsoft.AspNetCore.Mvc.Rendering;

namespace OrchardCore.Workflows.Http.ViewModels;

public class HttpRequestEventViewModel
{
    private static readonly IEnumerable<string> _availableHttpMethods = new[]
        {
            "GET",
            "POST",
            "PUT",
            "PATCH",
            "DELETE",
            "OPTIONS"
        };

    public string HttpMethod { get; set; }

    public string Url { get; set; }

    public bool ValidateAntiforgeryToken { get; set; }

    public int TokenLifeSpan { get; set; }

    public string FormLocationKey { get; set; }

    public static IList<SelectListItem> GetAvailableHttpMethods()
        => _availableHttpMethods.Select(x => new SelectListItem { Text = x, Value = x }).ToList();
}
