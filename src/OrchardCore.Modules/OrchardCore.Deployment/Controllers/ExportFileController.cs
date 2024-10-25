using System.IO.Compression;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrchardCore.Admin;
using OrchardCore.Deployment.Core.Mvc;
using OrchardCore.Deployment.Core.Services;
using OrchardCore.Deployment.Services;
using OrchardCore.Deployment.Steps;
using OrchardCore.Mvc.Utilities;
using OrchardCore.Recipes.Models;
using YesSql;

namespace OrchardCore.Deployment.Controllers;

[Admin("DeploymentPlan/ExportFile/{action}/{id?}", "DeploymentPlanExportFile{action}")]
public sealed class ExportFileController : Controller
{
    private readonly IDeploymentManager _deploymentManager;
    private readonly IAuthorizationService _authorizationService;
    private readonly ISession _session;

    public ExportFileController(
        IAuthorizationService authorizationService,
        ISession session,
        IDeploymentManager deploymentManager)
    {
        _authorizationService = authorizationService;
        _deploymentManager = deploymentManager;
        _session = session;
    }

    [HttpPost]
    [DeleteFileResultFilter]
    public async Task<IActionResult> Execute(long id)
    {
        if (!await _authorizationService.AuthorizeAsync(User, CommonPermissions.Export))
        {
            return Forbid();
        }

        var deploymentPlan = await _session.GetAsync<DeploymentPlan>(id);

        if (deploymentPlan == null)
        {
            return NotFound();
        }

        string archiveFileName;
        var filename = deploymentPlan.Name.ToSafeName() + ".zip";

        using (var fileBuilder = new TemporaryFileBuilder())
        {
            archiveFileName = fileBuilder.Folder + ".zip";

            var recipeDescriptor = new RecipeDescriptor();
            var recipeFileDeploymentStep = deploymentPlan.DeploymentSteps.FirstOrDefault(ds => ds.Name == nameof(RecipeFileDeploymentStep)) as RecipeFileDeploymentStep;

            if (recipeFileDeploymentStep != null)
            {
                recipeDescriptor.Name = recipeFileDeploymentStep.RecipeName;
                recipeDescriptor.DisplayName = recipeFileDeploymentStep.DisplayName;
                recipeDescriptor.Description = recipeFileDeploymentStep.Description;
                recipeDescriptor.Author = recipeFileDeploymentStep.Author;
                recipeDescriptor.WebSite = recipeFileDeploymentStep.WebSite;
                recipeDescriptor.Version = recipeFileDeploymentStep.Version;
                recipeDescriptor.IsSetupRecipe = recipeFileDeploymentStep.IsSetupRecipe;
                recipeDescriptor.Categories = (recipeFileDeploymentStep.Categories ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries);
                recipeDescriptor.Tags = (recipeFileDeploymentStep.Tags ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries);
            }

            var deploymentPlanResult = new DeploymentPlanResult(fileBuilder, recipeDescriptor);
            await _deploymentManager.ExecuteDeploymentPlanAsync(deploymentPlan, deploymentPlanResult);
            ZipFile.CreateFromDirectory(fileBuilder.Folder, archiveFileName);
        }

        return new PhysicalFileResult(archiveFileName, "application/zip") { FileDownloadName = filename };
    }
}
