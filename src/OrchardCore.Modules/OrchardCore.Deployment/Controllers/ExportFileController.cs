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
    [Admin]
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
        public async Task<IActionResult> Execute(int id)
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
                archiveFileName = PathExtensions.Combine(Path.GetTempPath(), filename);

                var recipeDescriptor = new RecipeDescriptor();
                CustomFileDeploymentStep customFileDeploymentStep = deploymentPlan.DeploymentSteps.FirstOrDefault(ds => ds.Name == "CustomFileDeploymentStep") as CustomFileDeploymentStep;

                if (customFileDeploymentStep != null)
                {
                    recipeDescriptor.Name = customFileDeploymentStep.RecipeName;
                    recipeDescriptor.DisplayName = customFileDeploymentStep.DisplayName;
                    recipeDescriptor.Description = customFileDeploymentStep.Description;
                    recipeDescriptor.Author = customFileDeploymentStep.Author;
                    recipeDescriptor.WebSite = customFileDeploymentStep.WebSite;
                    recipeDescriptor.Version = customFileDeploymentStep.Version;
                    recipeDescriptor.IsSetupRecipe = customFileDeploymentStep.IsSetupRecipe;
                    recipeDescriptor.Categories = (customFileDeploymentStep.Categories ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries);
                    recipeDescriptor.Tags = (customFileDeploymentStep.Tags ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries);
                }

                var deploymentPlanResult = new DeploymentPlanResult(fileBuilder, recipeDescriptor);
                await _deploymentManager.ExecuteDeploymentPlanAsync(deploymentPlan, deploymentPlanResult);
                ZipFile.CreateFromDirectory(fileBuilder.Folder, archiveFileName);
            }

            return new PhysicalFileResult(archiveFileName, "application/zip") { FileDownloadName = filename };
        }
    }
}
