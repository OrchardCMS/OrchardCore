using Microsoft.Extensions.FileProviders;
using OrchardCore.Deployment.Services;
using OrchardCore.FileStorage;
using OrchardCore.Jobs;
using OrchardCore.Recipes.Models;

namespace OrchardCore.Deployment.Jobs;

/// <summary>
/// The parameters of a run of <see cref="DeploymentImportJob"/>.
/// </summary>
public sealed class DeploymentImportJobParameters
{
    /// <summary>
    /// Gets or sets the temporary folder the deployment package was extracted to, on the server that received it.
    /// </summary>
    public string Folder { get; set; }
}

/// <summary>
/// Imports a deployment package uploaded from the admin in the background, so a large package doesn't make the
/// request wait. The run is executed by the server that received the package, which holds its files, and deletes
/// them when it ends. It is registered when the Jobs feature is enabled.
/// </summary>
public sealed class DeploymentImportJob : IJob
{
    /// <summary>
    /// The name of the job.
    /// </summary>
    public const string Name = "DeploymentImport";

    private readonly IDeploymentManager _deploymentManager;
    private readonly ITempDirectoryProvider _tempDirectoryProvider;

    public DeploymentImportJob(IDeploymentManager deploymentManager, ITempDirectoryProvider tempDirectoryProvider)
    {
        _deploymentManager = deploymentManager;
        _tempDirectoryProvider = tempDirectoryProvider;
    }

    public async Task ExecuteAsync(JobExecutionContext context, CancellationToken cancellationToken)
    {
        var parameters = context.GetParameters<DeploymentImportJobParameters>();
        var folder = string.IsNullOrEmpty(parameters.Folder) ? null : Path.GetFullPath(parameters.Folder);
        var root = Path.GetFullPath(_tempDirectoryProvider.GetRootDirectory());

        // Only a folder of the temporary files of the tenant is imported, and deleted.
        if (folder is null || !folder.StartsWith(root.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("The folder of the deployment package is not a temporary folder of the tenant.");
        }

        try
        {
            if (!Directory.Exists(folder))
            {
                throw new InvalidOperationException("The files of the deployment package don't exist anymore on this server.");
            }

            context.ReportProgress(null, "Importing the deployment package.");

            try
            {
                await _deploymentManager.ImportDeploymentPackageAsync(new PhysicalFileProvider(folder));
            }
            catch (RecipeExecutionException e)
            {
                foreach (var error in e.StepResult.Errors)
                {
                    context.LogError(error);
                }

                throw new InvalidOperationException($"The import failed with the following errors: {string.Join(' ', e.StepResult.Errors)}", e);
            }

            context.LogInformation("The deployment package was imported.");
        }
        finally
        {
            if (Directory.Exists(folder))
            {
                Directory.Delete(folder, recursive: true);
            }
        }
    }
}
