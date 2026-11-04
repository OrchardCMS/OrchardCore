using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using OrchardCore.Deployment.Remote.Services;
using OrchardCore.Deployment.Core.Services;
using OrchardCore.Deployment.Remote.ViewModels;
using OrchardCore.Deployment.Services;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.FileStorage;
using OrchardCore.Recipes.Models;

namespace OrchardCore.Deployment.Remote.Controllers;

public sealed class ImportRemoteInstanceController : Controller
{
    private readonly RemoteClientService _remoteClientService;
    private readonly IDeploymentManager _deploymentManager;
    private readonly INotifier _notifier;
    private readonly ILogger _logger;
    private readonly FileCreationService _fileCreationService;
    private readonly DeploymentPackageService _packages;

    internal readonly IHtmlLocalizer H;
    internal readonly IStringLocalizer S;

    public ImportRemoteInstanceController(
        RemoteClientService remoteClientService,
        IDeploymentManager deploymentManager,
        FileCreationService fileCreationService,
        DeploymentPackageService packages,
        INotifier notifier,
        IHtmlLocalizer<ImportRemoteInstanceController> htmlLocalizer,
        IStringLocalizer<ImportRemoteInstanceController> stringLocalizer,
        ILogger<ImportRemoteInstanceController> logger)
    {
        _deploymentManager = deploymentManager;
        _fileCreationService = fileCreationService;
        _packages = packages;
        _notifier = notifier;
        _logger = logger;
        _remoteClientService = remoteClientService;
        H = htmlLocalizer;
        S = stringLocalizer;
    }

    /// <remarks>
    /// We ignore the AFT as the service is called from external applications (they can't have valid ones) and
    /// we use a private API key to secure its calls.
    /// </remarks>
    [HttpPost]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> Import(ImportViewModel model)
    {
        if (model.Content is null || string.IsNullOrWhiteSpace(model.ClientName) || string.IsNullOrEmpty(model.ApiKey))
        {
            return BadRequest();
        }
        var remoteClientList = await _remoteClientService.GetRemoteClientListAsync();

        var remoteClient = remoteClientList.RemoteClients.FirstOrDefault(x => x.ClientName == model.ClientName);

        if (remoteClient == null)
        {
            return StatusCode((int)HttpStatusCode.BadRequest, "The remote client was not provided");
        }

        if (!_remoteClientService.MatchesApiKey(remoteClient, model.ApiKey))
        {
            return StatusCode((int)HttpStatusCode.BadRequest, "The Api Key was not recognized");
        }

        try
        {
            await using var uploadedStream = model.Content.OpenReadStream();
            await using var fileCreatingResult = await _fileCreationService.CreateAsync(
                new FileCreatingContext(model.Content.FileName, model.Content.Length, model.Content.ContentType),
                uploadedStream,
                HttpContext.RequestAborted);

            if (!fileCreatingResult.Succeeded)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, fileCreatingResult.ErrorMessage ?? $"The uploaded file '{model.Content.FileName}' was rejected.");
            }

            using var package = await _packages.StageAsync(fileCreatingResult.Stream, model.Content.FileName, HttpContext.RequestAborted);
            await _deploymentManager.ImportDeploymentPackageAsync(package.FileProvider);
        }
        catch (RecipeExecutionException e)
        {
            _logger.LogError(e, "Unable to import a recipe from deployment plan.");

            await _notifier.ErrorAsync(H["The deployment plan failed with the following errors: {0}", string.Join(' ', e.StepResult.Errors)]);
            return StatusCode(500);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Unexpected error occurred while executing a deployment plan.");

            await _notifier.ErrorAsync(H["Unexpected error occurred while executing a deployment plan."]);
            return StatusCode(500);
        }

        return Ok();
    }
}
