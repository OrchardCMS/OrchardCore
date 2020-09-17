using Microsoft.AspNetCore.Mvc;
using OrchardCore.FileStorage.AzureBlob;
using OrchardCore.Modules;

namespace OrchardCore.Media.Azure
{
    [Feature("OrchardCore.Media.Azure.Storage")]
    public class AdminController : Controller
    {
        private readonly BlobStorageOptions _options;

        public AdminController(BlobStorageOptions options)
        {
            _options = options;
        }

        public IActionResult Options()
        {
            return View(_options);
        }
    }
}
