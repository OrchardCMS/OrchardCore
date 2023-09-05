using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Localization;
using OrchardCore.Secrets.Models;
using OrchardCore.Secrets.ViewModels;

namespace OrchardCore.Secrets.ViewComponents;

public class SelectSecretViewComponent : ViewComponent
{
    private readonly ISecretService _secretService;
    protected readonly IStringLocalizer S;

    public SelectSecretViewComponent(
        ISecretService secretService,
        IStringLocalizer<SelectSecretViewComponent> stringLocalizer)
    {
        _secretService = secretService;
        S = stringLocalizer;
    }

    public async Task<IViewComponentResult> InvokeAsync(string secretType, string selectedSecret, string htmlId, string htmlName, bool required)
    {
        var secretBindings = await _secretService.GetSecretBindingsAsync();

        var secrets = secretBindings
            .Where(kv => String.Equals(secretType, kv.Value.Type, StringComparison.OrdinalIgnoreCase))
            .Select(kv => new SelectListItem()
            {
                Text = kv.Key,
                Value = kv.Key,
                Selected = String.Equals(kv.Key, selectedSecret, StringComparison.OrdinalIgnoreCase),
            })
            .ToList();

        if (!required)
        {
            if (secretType == typeof(TextSecret).Name)
            {
                secrets.Insert(0, new SelectListItem() { Text = S["None - Use plain text instead"], Value = String.Empty });
            }
            else
            {
                secrets.Insert(0, new SelectListItem() { Text = S["None"], Value = String.Empty });
            }
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
