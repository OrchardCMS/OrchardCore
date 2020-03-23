using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.FileProviders;
using OrchardCore.Admin;
using OrchardCore.Deployment.Services;
using OrchardCore.DisplayManagement.Notify;

namespace OrchardCore.Deployment.Controllers
{
    [Admin]
    public class ImportController : Controller
    {
        private readonly IDeploymentManager _deploymentManager;
        private readonly IAuthorizationService _authorizationService;
        private readonly INotifier _notifier;
        private readonly IHtmlLocalizer H;

        public ImportController(
            IDeploymentManager deploymentManager,
            IAuthorizationService authorizationService,
            INotifier notifier,
            IHtmlLocalizer<ImportController> localizer
        )
        {
            _deploymentManager = deploymentManager;
            _authorizationService = authorizationService;
            _notifier = notifier;

            H = localizer;
        }

        public async Task<IActionResult> Index()
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.Import))
            {
                return Forbid();
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Import(IFormFile importedPackage)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.Import))
            {
                return Forbid();
            }

            if (importedPackage != null)
            {
                var tempArchiveName = Path.GetTempFileName() + Path.GetExtension(importedPackage.FileName);
                var tempArchiveFolder = PathExtensions.Combine(Path.GetTempPath(), Path.GetRandomFileName());

                try
                {
                    using (var stream = new FileStream(tempArchiveName, FileMode.Create))
                    {
                        await importedPackage.CopyToAsync(stream);
                    }

                    if (importedPackage.FileName.EndsWith(".zip"))
                    {
                        ZipFile.ExtractToDirectory(tempArchiveName, tempArchiveFolder);
                    }

                    if (importedPackage.FileName.EndsWith(".json"))
                    {
                        Directory.CreateDirectory(tempArchiveFolder);
                        System.IO.File.Move(tempArchiveName, Path.Combine(tempArchiveFolder, "Recipe.json"));
                    }

                    await _deploymentManager.ImportDeploymentPackageAsync(new PhysicalFileProvider(tempArchiveFolder));

                    _notifier.Success(H["Deployment package imported"]);
                }
                finally
                {
                    if (System.IO.File.Exists(tempArchiveName))
                    {
                        System.IO.File.Delete(tempArchiveName);
                    }

                    if (Directory.Exists(tempArchiveFolder))
                    {
                        Directory.Delete(tempArchiveFolder, true);
                    }
                }
            }
            else
            {
                _notifier.Error(H["Please add a file to import."]);
            }

            return RedirectToAction("Index");
        }
    }
}
