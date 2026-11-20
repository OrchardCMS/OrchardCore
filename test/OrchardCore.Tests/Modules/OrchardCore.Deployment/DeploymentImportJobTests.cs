using OrchardCore.Deployment.Jobs;
using OrchardCore.FileStorage;
using OrchardCore.Jobs;
using OrchardCore.Jobs.Models;
using OrchardCore.Settings;
using OrchardCore.Tests.Modules.OrchardCore.Jobs;

namespace OrchardCore.Tests.Modules.OrchardCore.Deployment;

public sealed class DeploymentImportJobTests
{
    [Fact]
    public async Task Run_PackageFolder_ImportsItAndDeletesItsFiles()
    {
        // Arrange
        using var context = await JobsTestSite.CreateAsync("OrchardCore.Deployment");
        string folder = null;
        string runId = null;

        await context.UsingTenantScopeAsync(async scope =>
        {
            folder = scope.ServiceProvider.GetRequiredService<ITempDirectoryProvider>().CreateTempSubdirectory();

            await File.WriteAllTextAsync(Path.Combine(folder, "Recipe.json"), """
                {
                  "name": "Imported",
                  "steps": [
                    { "name": "settings", "SiteName": "Imported by a job" }
                  ]
                }
                """);

            // Act
            runId = (await scope.ServiceProvider.GetRequiredService<IJobManager>().EnqueueAsync<DeploymentImportJob>(
                new DeploymentImportJobParameters { Folder = folder },
                request => request.RequireCurrentNode = true)).RunId;
        });

        var run = await JobsTestSite.WaitForRunAsync(context, runId);

        // Assert
        Assert.True(run.Status == JobRunStatus.Succeeded, run.Error);
        Assert.False(Directory.Exists(folder));
        Assert.False(string.IsNullOrEmpty(run.RequiredNodeId));

        await context.UsingTenantScopeAsync(async scope =>
        {
            var site = await scope.ServiceProvider.GetRequiredService<ISiteService>().GetSiteSettingsAsync();

            Assert.Equal("Imported by a job", site.SiteName);
        });
    }

    [Fact]
    public async Task Run_FolderOutsideOfTheTemporaryFiles_Fails()
    {
        // Arrange
        using var context = await JobsTestSite.CreateAsync("OrchardCore.Deployment");
        var folder = Directory.CreateTempSubdirectory("job-import-test").FullName;
        string runId = null;

        try
        {
            // Act
            await context.UsingTenantScopeAsync(async scope =>
            {
                runId = (await scope.ServiceProvider.GetRequiredService<IJobManager>().EnqueueAsync<DeploymentImportJob>(new DeploymentImportJobParameters { Folder = folder })).RunId;
            });

            var run = await JobsTestSite.WaitForRunAsync(context, runId);

            // Assert
            Assert.Equal(JobRunStatus.Failed, run.Status);
            Assert.True(Directory.Exists(folder));
        }
        finally
        {
            Directory.Delete(folder, recursive: true);
        }
    }
}
