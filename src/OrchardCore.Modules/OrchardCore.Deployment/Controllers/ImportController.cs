using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Localization;
using OrchardCore.Admin;
using OrchardCore.Deployment.Services;
using OrchardCore.Deployment.ViewModels;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Mvc.Utilities;

namespace OrchardCore.Deployment.Controllers
{
    [Admin]
    public class ImportController : Controller
    {
        private readonly IDeploymentManager _deploymentManager;
        private readonly IAuthorizationService _authorizationService;
        private readonly INotifier _notifier;
        protected readonly IHtmlLocalizer H;
        protected readonly IStringLocalizer S;

        public ImportController(
            IDeploymentManager deploymentManager,
            IAuthorizationService authorizationService,
            INotifier notifier,
            IHtmlLocalizer<ImportController> htmlLocalizer,
            IStringLocalizer<ImportController> stringLocalizer
        )
        {
            _deploymentManager = deploymentManager;
            _authorizationService = authorizationService;
            _notifier = notifier;

            H = htmlLocalizer;
            S = stringLocalizer;
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
                    else if (importedPackage.FileName.EndsWith(".json"))
                    {
                        Directory.CreateDirectory(tempArchiveFolder);
                        System.IO.File.Move(tempArchiveName, Path.Combine(tempArchiveFolder, "Recipe.json"));
                    }
                    else
                    {
                        await _notifier.ErrorAsync(H["Only zip or json files are supported."]);

                        return RedirectToAction(nameof(Index));
                    }

                    await _deploymentManager.ImportDeploymentPackageAsync(new PhysicalFileProvider(tempArchiveFolder));

                    await _notifier.SuccessAsync(H["Deployment package imported."]);
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
                await _notifier.ErrorAsync(H["Please add a file to import."]);
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Json()
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.Import))
            {
                return Forbid();
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Json(ImportJsonViewModel model)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.Import))
            {
                return Forbid();
            }

            if (!model.Json.IsJson())
            {
                ModelState.AddModelError(nameof(model.Json), S["The recipe is written in an incorrect json format."]);
            }

            if (ModelState.IsValid)
            {
                var tempArchiveFolder = PathExtensions.Combine(Path.GetTempPath(), Path.GetRandomFileName());

                try
                {
                    Directory.CreateDirectory(tempArchiveFolder);
                    System.IO.File.WriteAllText(Path.Combine(tempArchiveFolder, "Recipe.json"), model.Json);

                    await _deploymentManager.ImportDeploymentPackageAsync(new PhysicalFileProvider(tempArchiveFolder));

                    await _notifier.SuccessAsync(H["Recipe imported."]);
                }
                finally
                {
                    if (Directory.Exists(tempArchiveFolder))
                    {
                        Directory.Delete(tempArchiveFolder, true);
                    }
                }
            }

            return View(model);
        }
    }
}
