using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrchardCore.Admin.Models;
using OrchardCore.Admin.QuickNavigation;
using OrchardCore.Settings;

namespace OrchardCore.Admin.Controllers;

[Authorize]
[Admin("QuickNavigation/Index", "AdminQuickNavigationIndex")]
public sealed class QuickNavigationController : Controller
{
    private readonly IQuickNavigationIndex _index;
    private readonly IDictionary<string, QuickNavigationSource> _sources;
    private readonly ISiteService _siteService;

    public QuickNavigationController(
        IQuickNavigationIndex index,
        IEnumerable<QuickNavigationSource> sources,
        ISiteService siteService)
    {
        _index = index;
        _sources = sources.ToDictionary(source => source.Name, StringComparer.Ordinal);
        _siteService = siteService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        Response.Headers.CacheControl = "no-store";
        if (!(await _siteService.GetSettingsAsync<AdminSettings>()).DisplayQuickNavigation)
        {
            return NotFound();
        }

        var results = new List<QuickNavigationResult>();
        foreach (var (name, source) in _sources)
        {
            var requestEntryIds = await source.GetEntryIdsAsync();
            var entryIds = _index.GetEntryIds(name)
                .Concat(requestEntryIds)
                .Distinct(StringComparer.Ordinal);

            foreach (var entryId in entryIds)
            {
                var result = await source.DisplayAsync(entryId);
                if (result != null)
                {
                    results.Add(new QuickNavigationResult
                    {
                        Source = name,
                        Id = entryId,
                        Title = result.Title,
                        Path = result.Path,
                        Href = result.Href,
                        Target = result.Target,
                    });
                }
            }
        }

        var payload = JsonSerializer.SerializeToUtf8Bytes(results, JsonSerializerOptions.Web);
        return File(payload, "application/json; charset=utf-8");
    }
}
