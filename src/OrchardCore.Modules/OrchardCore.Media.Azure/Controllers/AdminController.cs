using Microsoft.AspNetCore.Mvc;
using OrchardCore.FileStorage.AzureBlob;

namespace OrchardCore.Media.Azure
{
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
