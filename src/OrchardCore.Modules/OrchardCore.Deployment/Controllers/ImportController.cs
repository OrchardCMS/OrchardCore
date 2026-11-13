using System.IO.Compression;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Admin;
using OrchardCore.Deployment.Jobs;
using OrchardCore.Deployment.Services;
using OrchardCore.Deployment.ViewModels;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.FileStorage;
using OrchardCore.Jobs;
using OrchardCore.Jobs.Models;
using OrchardCore.Mvc.Utilities;
using OrchardCore.Recipes.Models;

namespace OrchardCore.Deployment.Controllers;

[Admin("DeploymentPlan/Import/{action}", "DeploymentPlanImport{action}")]
public sealed class ImportController : Controller
{
    private readonly IDeploymentManager _deploymentManager;
    private readonly IAuthorizationService _authorizationService;
    private readonly INotifier _notifier;
    private readonly ILogger _logger;
    private readonly FileCreationService _fileCreationService;
    private readonly ITempDirectoryProvider _tempDirectoryProvider;
    private readonly IEnumerable<IJobManager> _jobManagers;
    private readonly JobOptions _jobOptions;

    internal readonly IHtmlLocalizer H;
    internal readonly IStringLocalizer S;

    public ImportController(
        IDeploymentManager deploymentManager,
        IAuthorizationService authorizationService,
        FileCreationService fileCreationService,
        ITempDirectoryProvider tempDirectoryProvider,
        INotifier notifier,
        ILogger<ImportController> logger,
        IEnumerable<IJobManager> jobManagers,
        IOptions<JobOptions> jobOptions,
        IHtmlLocalizer<ImportController> htmlLocalizer,
        IStringLocalizer<ImportController> stringLocalizer
    )
    {
        _deploymentManager = deploymentManager;
        _authorizationService = authorizationService;
        _fileCreationService = fileCreationService;
        _tempDirectoryProvider = tempDirectoryProvider;
        _notifier = notifier;
        _logger = logger;

        // The Jobs feature is optional: without it, no job manager is registered.
        _jobManagers = jobManagers;
        _jobOptions = jobOptions.Value;
        H = htmlLocalizer;
        S = stringLocalizer;
    }

    public async Task<IActionResult> Index()
    {
        if (!await _authorizationService.AuthorizeAsync(User, DeploymentPermissions.Import))
        {
            return Forbid();
        }

        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Import(IFormFile importedPackage)
    {
        if (!await _authorizationService.AuthorizeAsync(User, DeploymentPermissions.Import))
        {
            return Forbid();
        }

        if (importedPackage != null)
        {
            var tempArchiveName = _tempDirectoryProvider.GetTempFileName(Path.GetExtension(importedPackage.FileName));
            var tempArchiveFolder = _tempDirectoryProvider.GetTempFileName();
            var queued = false;

            try
            {
                await using var uploadedStream = importedPackage.OpenReadStream();
                await using var fileCreatingResult = await _fileCreationService.CreateAsync(
                    new FileCreatingContext(importedPackage.FileName, importedPackage.Length, importedPackage.ContentType),
                    uploadedStream,
                    HttpContext.RequestAborted);

                if (!fileCreatingResult.Succeeded)
                {
#pragma warning disable OCLOC104 // The error message comes from the file creation service, which localizes it.
                    await _notifier.ErrorAsync(H[fileCreatingResult.ErrorMessage ?? $"The uploaded file '{importedPackage.FileName}' was rejected."]);
#pragma warning restore OCLOC104

                    return RedirectToAction(nameof(Index));
                }

                await using (var stream = new FileStream(tempArchiveName, FileMode.Create))
                {
                    await fileCreatingResult.Stream.CopyToAsync(stream, HttpContext.RequestAborted);
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

                // With the Jobs feature, the package is imported in the background: a large package doesn't make the
                // request wait, and its run tells how it went. This server holds its files, so it runs it.
                if (_jobManagers.LastOrDefault() is { } jobManager && _jobOptions.Find(DeploymentImportJob.Name) is not null)
                {
                    var run = await jobManager.EnqueueAsync<DeploymentImportJob>(new DeploymentImportJobParameters { Folder = tempArchiveFolder }, request =>
                    {
                        request.Title = Path.GetFileName(importedPackage.FileName);
                        request.Trigger = JobTriggers.Manual;
                        request.RequireCurrentNode = true;
                    });

                    // The run deletes the files of the package when it ends.
                    queued = true;

                    await _notifier.SuccessAsync(H["The deployment package is imported in the background. <a href=\"{0}\">View its run</a>.", Url.Action("Details", "JobRun", new { area = "OrchardCore.Jobs", runId = run.RunId })]);

                    return RedirectToAction(nameof(Index));
                }

                await _deploymentManager.ImportDeploymentPackageAsync(new PhysicalFileProvider(tempArchiveFolder));

                await _notifier.SuccessAsync(H["Deployment package imported."]);
            }
            catch (RecipeExecutionException e)
            {
                _logger.LogError(e, "Unable to import a deployment package.");

                await _notifier.ErrorAsync(H["The import failed with the following errors: {0}", string.Join(' ', e.StepResult.Errors)]);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Unable to import a deployment package.");

                await _notifier.ErrorAsync(H["Unexpected error occurred while importing the deployment package."]);
            }
            finally
            {
                if (System.IO.File.Exists(tempArchiveName))
                {
                    System.IO.File.Delete(tempArchiveName);
                }

                if (!queued && Directory.Exists(tempArchiveFolder))
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
        if (!await _authorizationService.AuthorizeAsync(User, DeploymentPermissions.Import))
        {
            return Forbid();
        }

        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Json(ImportJsonViewModel model)
    {
        if (!await _authorizationService.AuthorizeAsync(User, DeploymentPermissions.Import))
        {
            return Forbid();
        }

        if (!model.Json.IsJson(JOptions.Document))
        {
            ModelState.AddModelError(nameof(model.Json), S["The recipe is written in an incorrect JSON format."]);
        }

        if (ModelState.IsValid)
        {
            var tempArchiveFolder = _tempDirectoryProvider.CreateTempSubdirectory();

            try
            {
                System.IO.File.WriteAllText(Path.Combine(tempArchiveFolder, "Recipe.json"), model.Json);

                await _deploymentManager.ImportDeploymentPackageAsync(new PhysicalFileProvider(tempArchiveFolder));

                await _notifier.SuccessAsync(H["Recipe imported successfully!"]);
            }
            catch (RecipeExecutionException e)
            {
                _logger.LogError(e, "Unable to import a recipe from JSON input.");

                ModelState.AddModelError(nameof(model.Json), string.Join(' ', e.StepResult.Errors));
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Unable to import a recipe from JSON input.");

                ModelState.AddModelError(string.Empty, S["Unexpected error occurred while importing the recipe."]);
            }
            finally
            {
                if (Directory.Exists(tempArchiveFolder))
                {
                    Directory.Delete(tempArchiveFolder, true);
                }
            }
        }

        return RedirectToAction(nameof(Json));
    }
}
