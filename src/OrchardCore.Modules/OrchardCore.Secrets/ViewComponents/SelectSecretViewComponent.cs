using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Localization;
using OrchardCore.Secrets.ViewModels;

namespace OrchardCore.Secrets.ViewComponents;

public class SelectSecretViewComponent : ViewComponent
{
    private readonly ISecretManager _secretManager;
    internal readonly IStringLocalizer S;

    public SelectSecretViewComponent(
        ISecretManager secretManager,
        IStringLocalizer<SelectSecretViewComponent> stringLocalizer)
    {
        _secretManager = secretManager;
        S = stringLocalizer;
    }

    public async Task<IViewComponentResult> InvokeAsync(
        string secretType,
        string selectedSecret,
        string htmlId,
        string htmlName,
        bool required = false)
    {
        var secretInfos = await _secretManager.GetSecretInfosAsync();

        var secrets = secretInfos
            .Where(info => string.IsNullOrEmpty(secretType) ||
                          string.Equals(secretType, info.Type, StringComparison.OrdinalIgnoreCase))
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
            Secrets = secrets,
        };

        return View(model);
    }
}
