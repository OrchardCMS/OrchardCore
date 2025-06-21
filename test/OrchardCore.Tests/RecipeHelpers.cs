using System.Text.Json;
using System.Text.Json.Nodes;
using OrchardCore.Deployment.Services;
using OrchardCore.Tests.Apis.Context;

namespace OrchardCore.Tests;

internal static class RecipeHelpers
{
    public static Task RunRecipeAsync(SiteContext context, JsonObject data)
    {
        return context.UsingTenantScopeAsync(async scope =>
        {
            var tempArchiveName = PathExtensions.GetTempFileName() + ".json";
            var tempArchiveFolder = PathExtensions.GetTempFileName();

            try
            {
                using (var stream = new FileStream(tempArchiveName, FileMode.Create))
                {
                    var bytes = JsonSerializer.SerializeToUtf8Bytes(data);

                    await stream.WriteAsync(bytes).ConfigureAwait(false);
                }

                Directory.CreateDirectory(tempArchiveFolder);
                File.Move(tempArchiveName, Path.Combine(tempArchiveFolder, "Recipe.json"));

                var deploymentManager = scope.ServiceProvider.GetRequiredService<IDeploymentManager>();

                await deploymentManager.ImportDeploymentPackageAsync(new PhysicalFileProvider(tempArchiveFolder)).ConfigureAwait(false);
            }
            finally
            {
                if (File.Exists(tempArchiveName))
                {
                    File.Delete(tempArchiveName);
                }

                if (Directory.Exists(tempArchiveFolder))
                {
                    Directory.Delete(tempArchiveFolder, true);
                }
            }
        });
    }
}
