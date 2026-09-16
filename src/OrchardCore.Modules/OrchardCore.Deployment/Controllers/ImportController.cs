using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using OrchardCore.Admin;
using OrchardCore.Deployment.Services;
using OrchardCore.Deployment.Core.Services;
using OrchardCore.Deployment.ViewModels;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.FileStorage;
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
    private readonly DeploymentPackageService _packages;

    internal readonly IHtmlLocalizer H;
    internal readonly IStringLocalizer S;

    /// <summary>Creates admin import actions with shared package validation and staging.</summary>
    public ImportController(
        IDeploymentManager deploymentManager,
        IAuthorizationService authorizationService,
        FileCreationService fileCreationService,
        DeploymentPackageService packages,
        INotifier notifier,
        ILogger<ImportController> logger,
        IHtmlLocalizer<ImportController> htmlLocalizer,
        IStringLocalizer<ImportController> stringLocalizer
    )
    {
        _deploymentManager = deploymentManager;
        _authorizationService = authorizationService;
        _fileCreationService = fileCreationService;
        _packages = packages;
        _notifier = notifier;
        _logger = logger;
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
            try
            {
                await using var uploadedStream = importedPackage.OpenReadStream();
                await using var fileCreatingResult = await _fileCreationService.CreateAsync(
                    new FileCreatingContext(importedPackage.FileName, importedPackage.Length, importedPackage.ContentType),
                    uploadedStream,
                    HttpContext.RequestAborted);

                if (!fileCreatingResult.Succeeded)
                {
                    await _notifier.ErrorAsync(H[fileCreatingResult.ErrorMessage ?? $"The uploaded file '{importedPackage.FileName}' was rejected."]);

                    return RedirectToAction(nameof(Index));
                }

                using var package = await _packages.StageAsync(fileCreatingResult.Stream, importedPackage.FileName, HttpContext.RequestAborted);
                await _deploymentManager.ImportDeploymentPackageAsync(package.FileProvider);

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
            try
            {
                using var input = new MemoryStream(Encoding.UTF8.GetBytes(model.Json));
                using var package = await _packages.StageAsync(input, "Recipe.json", HttpContext.RequestAborted);
                await _deploymentManager.ImportDeploymentPackageAsync(package.FileProvider);

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

        }

        return RedirectToAction(nameof(Json));
    }
}
