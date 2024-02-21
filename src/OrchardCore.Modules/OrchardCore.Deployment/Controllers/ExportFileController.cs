using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
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

namespace OrchardCore.Deployment.Controllers
{
    public class ExportFileController : Controller
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
        [Admin("DeploymentPlan/ExportFile/Execute/{id}", "DeploymentPlanExportFileExecute")]
        public async Task<IActionResult> Execute(long id)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.Export))
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

                var recipeDescriptor = GetRecipeDescriptor(deploymentPlan);

                var deploymentPlanResult = new DeploymentPlanResult(fileBuilder, recipeDescriptor);
                await _deploymentManager.ExecuteDeploymentPlanAsync(deploymentPlan, deploymentPlanResult);
                ZipFile.CreateFromDirectory(fileBuilder.Folder, archiveFileName);
            }

            return new PhysicalFileResult(archiveFileName, "application/zip") { FileDownloadName = filename };
        }

        [HttpPost]
        [DeleteFileResultFilter]
        [Admin("DeploymentPlan/ExecuteJson/Execute/{id}", "DeploymentPlanExportFileExecuteJson")]
        public async Task<IActionResult> ExecuteJson(long id)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.Export))
            {
                return Forbid();
            }

            var deploymentPlan = await _session.GetAsync<DeploymentPlan>(id);

            if (deploymentPlan == null)
            {
                return NotFound();
            }

            var filename = deploymentPlan.Name.ToSafeName() + ".json";
            var recipeDescriptor = GetRecipeDescriptor(deploymentPlan);

            var stream = new MemoryStream();
            var deploymentPlanResult = new MemoryDeploymentPlanResult(stream, recipeDescriptor);
            await _deploymentManager.ExecuteDeploymentPlanAsync(deploymentPlan, deploymentPlanResult);
            stream.Seek(0, SeekOrigin.Begin);

            return new FileStreamResult(stream, "application/json")
            {
                FileDownloadName = filename
            };
        }

        private static RecipeDescriptor GetRecipeDescriptor(DeploymentPlan deploymentPlan)
        {
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
                recipeDescriptor.Categories = (recipeFileDeploymentStep.Categories ?? string.Empty).Split(',', StringSplitOptions.RemoveEmptyEntries);
                recipeDescriptor.Tags = (recipeFileDeploymentStep.Tags ?? string.Empty).Split(',', StringSplitOptions.RemoveEmptyEntries);
            }

            return recipeDescriptor;
        }
    }
}
