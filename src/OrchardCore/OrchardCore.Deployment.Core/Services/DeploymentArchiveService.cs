using System.IO.Compression;
using OrchardCore.Deployment.Services;
using OrchardCore.FileStorage;
using OrchardCore.Recipes.Models;

namespace OrchardCore.Deployment.Core.Services;

/// <summary>Owns temporary staging and archive cleanup for deployment exports.</summary>
public sealed class DeploymentArchiveService : IDeploymentArchiveService
{
    private readonly IDeploymentManager _manager;
    private readonly ITempDirectoryProvider _temporary;

    /// <summary>Creates an archive service with tenant deployment sources and temporary storage.</summary>
    public DeploymentArchiveService(IDeploymentManager manager, ITempDirectoryProvider temporary)
    {
        _manager = manager;
        _temporary = temporary;
    }

    /// <inheritdoc />
    public async Task<Stream> CreateAsync(DeploymentPlan plan, RecipeDescriptor recipe)
    {
        ArgumentNullException.ThrowIfNull(plan);
        ArgumentNullException.ThrowIfNull(recipe);
        var builder = new TemporaryFileBuilder(_temporary.GetRootDirectory());
        var archive = builder.Folder + ".zip";
        try
        {
            using (builder)
            {
                await _manager.ExecuteDeploymentPlanAsync(plan, new DeploymentPlanResult(builder, recipe));
                ZipFile.CreateFromDirectory(builder.Folder, archive);
            }
            return new FileStream(archive, FileMode.Open, FileAccess.Read, FileShare.Read,
                4096, FileOptions.Asynchronous | FileOptions.SequentialScan | FileOptions.DeleteOnClose);
        }
        catch
        {
            File.Delete(archive);
            throw;
        }
    }
}
