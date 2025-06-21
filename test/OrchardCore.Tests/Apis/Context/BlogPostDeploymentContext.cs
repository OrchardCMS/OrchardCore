using System.Text.Json;
using System.Text.Json.Nodes;
using OrchardCore.ContentManagement;
using OrchardCore.Deployment.Remote.Services;
using OrchardCore.Deployment.Remote.ViewModels;

namespace OrchardCore.Tests.Apis.Context;

public class BlogPostDeploymentContext : SiteContext
{
    public const string RemoteDeploymentClientName = "testserver";
    public const string RemoteDeploymentApiKey = "testkey";
    public string BlogPostContentItemId { get; private set; }
    public ContentItem OriginalBlogPost { get; private set; }
    public string OriginalBlogPostVersionId { get; private set; }

    static BlogPostDeploymentContext()
    {
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync().ConfigureAwait(false);
        await RunRecipeAsync(BlogContext.luceneRecipeName, BlogContext.luceneRecipePath).ConfigureAwait(false);

        var result = await GraphQLClient
            .Content
            .Query("blogPost", builder =>
            {
                builder
                    .WithField("contentItemId");
            }).ConfigureAwait(false);

        BlogPostContentItemId = result["data"]["blogPost"][0]["contentItemId"].ToString();

        var content = await Client.GetAsync($"api/content/{BlogPostContentItemId}").ConfigureAwait(false);
        OriginalBlogPost = await content.Content.ReadAsAsync<ContentItem>().ConfigureAwait(false);
        OriginalBlogPostVersionId = OriginalBlogPost.ContentItemVersionId;

        await UsingTenantScopeAsync(async scope =>
        {
            var remoteClientService = scope.ServiceProvider.GetRequiredService<RemoteClientService>();

            await remoteClientService.CreateRemoteClientAsync(RemoteDeploymentClientName, RemoteDeploymentApiKey).ConfigureAwait(false);
        }).ConfigureAwait(false);
    }

    public static JsonObject GetContentStepRecipe(ContentItem contentItem, Action<JsonObject> mutation)
    {
        var jContentItem = JObject.FromObject(contentItem);
        mutation.Invoke(jContentItem);

        var recipe = new JsonObject
        {
            ["steps"] = new JsonArray
            {
                new JsonObject
                {
                    ["name"] = "content",
                    ["Data"] = new JsonArray { jContentItem },
                },
            },
        };

        return recipe;
    }

    public async Task<HttpResponseMessage> PostRecipeAsync(JsonObject recipe, bool ensureSuccess = true)
    {
        var zipStream = MemoryStreamFactory.GetStream();
        await using (zipStream.ConfigureAwait(false))
        {
            using (var zip = new ZipArchive(zipStream, ZipArchiveMode.Create, true))
        {
            var entry = zip.CreateEntry("Recipe.json");
            using var streamWriter = new Utf8JsonWriter(entry.Open());
            recipe.WriteTo(streamWriter);
        }

        zipStream.Seek(0, SeekOrigin.Begin);

        using var requestContent = new MultipartFormDataContent
        {
            { new StreamContent(zipStream), nameof(ImportViewModel.Content), "Recipe.zip" },
            { new StringContent(RemoteDeploymentClientName), nameof(ImportViewModel.ClientName) },
            { new StringContent(RemoteDeploymentApiKey), nameof(ImportViewModel.ApiKey) },
        };

        var response = await Client.PostAsync("OrchardCore.Deployment.Remote/ImportRemoteInstance/Import", requestContent).ConfigureAwait(false);
        if (ensureSuccess)
        {
            response.EnsureSuccessStatusCode();
        }

        return response;
        }
    }
}
