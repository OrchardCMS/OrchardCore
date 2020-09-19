using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using OrchardCore.Media.Azure.ViewModels;
using OrchardCore.Modules;

namespace OrchardCore.Media.Azure
{
    [Feature("OrchardCore.Media.Azure.Storage")]
    public class AdminController : Controller
    {
        private readonly MediaBlobStorageOptions _options;

        public AdminController(IOptions<MediaBlobStorageOptions> options)
        {
            _options = options.Value;
        }

        public IActionResult Options()
        {
            var model = new OptionsViewModel
            {
                CreateContainer = _options.CreateContainer,
                ContainerName = _options.ContainerName,
                BasePath = _options.BasePath
            };

            var indexPassword = _options.ConnectionString.IndexOf("password=", System.StringComparison.OrdinalIgnoreCase);
            model.ConnectionString = (indexPassword > -1) ? _options.ConnectionString.Substring(0, indexPassword) + "&bull;&bull;&bull;" : _options.ConnectionString;

            return View(model);
        }
    }
}
