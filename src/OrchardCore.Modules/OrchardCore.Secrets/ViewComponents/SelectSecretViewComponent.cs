using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Localization;
using OrchardCore.Secrets.ViewModels;

namespace OrchardCore.Secrets.ViewComponents;

public class SelectSecretViewComponent : ViewComponent
{
    private readonly ISecretManager _secretManager;
    private readonly IEnumerable<ISecretTypeProvider> _providers;
    internal readonly IStringLocalizer S;

    public SelectSecretViewComponent(
        ISecretManager secretManager,
        IEnumerable<ISecretTypeProvider> providers,
        IStringLocalizer<SelectSecretViewComponent> stringLocalizer)
    {
        _secretManager = secretManager;
        _providers = providers;
        S = stringLocalizer;
    }

    public async Task<IViewComponentResult> InvokeAsync(
        string selectedSecret,
        string htmlId,
        string htmlName,
        IEnumerable<string> secretTypes = null,
        bool required = false,
        string cssClass = null)
    {
        var secretInfos = await _secretManager.GetSecretInfosAsync();

        var allowedTypes = secretTypes?.ToList() ?? [];

        var secrets = secretInfos
            .Where(info => allowedTypes.Count == 0 ||
                          allowedTypes.Any(t => MatchesType(t, info.Type)))
            .Select(info => new SelectListItem
            {
                Text = info.Name,
                Value = info.Name,
                Selected = string.Equals(info.Name, selectedSecret, StringComparison.OrdinalIgnoreCase),
            })
            .ToList();

        if (!required)
        {
            secrets.Insert(0, new SelectListItem { Text = S["None"], Value = string.Empty });
        }

        var model = new SelectSecretViewModel
        {
            HtmlId = htmlId,
            HtmlName = htmlName,
            CssClass = cssClass,
            Secrets = secrets,
        };

        return View(model);
    }

    private bool MatchesType(string requested, string stored)
    {
        if (string.Equals(requested, stored, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return _providers.Any(p =>
            (string.Equals(requested, p.Name, StringComparison.OrdinalIgnoreCase) ||
             string.Equals(requested, p.SecretType.FullName, StringComparison.OrdinalIgnoreCase)) &&
            (string.Equals(stored, p.Name, StringComparison.OrdinalIgnoreCase) ||
             string.Equals(stored, p.SecretType.FullName, StringComparison.OrdinalIgnoreCase)));
    }
}
