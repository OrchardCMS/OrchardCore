using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using OrchardCore.Secrets.ViewModels;

namespace OrchardCore.Secrets.ViewComponents
{
    public class SelectSecretViewComponent : ViewComponent
    {
        private readonly ISecretCoordinator _secretCoordinator;

        public SelectSecretViewComponent(ISecretCoordinator secretCoordinator)
        {
            _secretCoordinator = secretCoordinator;
        }

        public async Task<IViewComponentResult> InvokeAsync(IEnumerable<string> types, string selectedSecret, string htmlName)
        {
            // TODO add optional
            var secretBindings = await _secretCoordinator.GetSecretBindingsAsync();
            var secrets = secretBindings.Where(x => types.Contains(x.Value.Type, StringComparer.OrdinalIgnoreCase))
                    .Select(x => new SelectListItem() { Text = x.Key, Value = x.Key }).ToList();

            var model = new SelectSecretViewModel
            {
                HtmlName = htmlName,
                SelectedSecret = selectedSecret,
                Secrets = secrets
            };

            return View(model);
        }
    }
}
