using System.Text.Json.Nodes;
using OrchardCore.Deployment.Services;
using OrchardCore.Tests.Apis.Context;

namespace OrchardCore.Tests.Modules.OrchardCore.OpenId;

public class OpenIdAuthenticationTests
{
    [Fact]
    public async Task OpenIdCanExchangeCodeForAccessToken()
    {

        var recipeSteps = new JsonArray
        {
            new JsonObject
            {
                {"name", "OpenIdApplication"},
                {"clientId", "id"},
                {"displayName", "Test Application"},
                {"type", "public"},
                {"consentType", "explicit"},
                {"allowClientCredentialsFlow", true},
                {"redirectUris", "https://localhost/redirect"},
                {"roleEntries", new JsonArray()
                    {
                        new JsonObject
                        {
                            {"name", "role1"},
                            {"selected", true},
                        },
                        new JsonObject
                        {
                            {"name", "role2"},
                        },
                        new JsonObject
                        {
                            {"name", "scope1"},
                            {"selected", true},
                        },
                    }
                },
            },
        };

        var recipe = new JsonObject
        {
            {"steps", recipeSteps},
        };

        var context = new SiteContext();

        await context.InitializeAsync();

        await RunRecipeAsync(context, recipe);


        // make request to the serve.
        try
        {
            var result = await context.Client.GetAsync("connect/authorize", CancellationToken.None);
        }
        catch (Exception ex)
        {
            var t = ex;
        }

    }

    private static async Task RunRecipeAsync(SiteContext context, JsonObject data)
    {
        await context.UsingTenantScopeAsync(async scope =>
        {
            var tempArchiveName = PathExtensions.GetTempFileName() + ".json";
            var tempArchiveFolder = PathExtensions.GetTempFileName();

            try
            {
                using (var stream = new FileStream(tempArchiveName, FileMode.Create))
                {
                    var bytes = Encoding.UTF8.GetBytes(data.ToJsonString());

                    await stream.WriteAsync(bytes);
                }

                Directory.CreateDirectory(tempArchiveFolder);
                File.Move(tempArchiveName, Path.Combine(tempArchiveFolder, "Recipe.json"));

                var deploymentManager = scope.ServiceProvider.GetRequiredService<IDeploymentManager>();

                await deploymentManager.ImportDeploymentPackageAsync(new PhysicalFileProvider(tempArchiveFolder));
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
