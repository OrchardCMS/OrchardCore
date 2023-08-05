using OrchardCore.ContentManagement;
using OrchardCore.Deployment.Remote.Services;
using OrchardCore.Deployment.Remote.ViewModels;

namespace OrchardCore.Tests.Apis.Context
{
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
            await base.InitializeAsync();
            await RunRecipeAsync(BlogContext.luceneRecipeName, BlogContext.luceneRecipePath);

            var result = await GraphQLClient
                .Content
                .Query("blogPost", builder =>
                {
                    builder
                        .WithField("contentItemId");
                });

            BlogPostContentItemId = result["data"]["blogPost"].First["contentItemId"].ToString();

            var content = await Client.GetAsync($"api/content/{BlogPostContentItemId}");
            OriginalBlogPost = await content.Content.ReadAsAsync<ContentItem>();
            OriginalBlogPostVersionId = OriginalBlogPost.ContentItemVersionId;

            await UsingTenantScopeAsync(async scope =>
            {
                var remoteClientService = scope.ServiceProvider.GetRequiredService<RemoteClientService>();

                await remoteClientService.CreateRemoteClientAsync(RemoteDeploymentClientName, RemoteDeploymentApiKey);
            });
        }

        public static JObject GetContentStepRecipe(ContentItem contentItem, Action<JObject> mutation)
        {
            var jContentItem = JObject.FromObject(contentItem);
            mutation.Invoke(jContentItem);

            var recipe = new JObject
            {
                ["steps"] = new JArray
                {
                    new JObject
                    {
                        ["name"] = "content",
                        ["Data"] = new JArray { jContentItem }
                    }
                }
            };

            return recipe;
        }

        public async Task<HttpResponseMessage> PostRecipeAsync(JObject recipe, bool ensureSuccess = true)
        {
            using var zipStream = new MemoryStream();
            using (var zip = new ZipArchive(zipStream, ZipArchiveMode.Create, true))
            {
                var entry = zip.CreateEntry("Recipe.json");
                using var streamWriter = new StreamWriter(entry.Open());
                using var jsonWriter = new JsonTextWriter(streamWriter);
                await recipe.WriteToAsync(jsonWriter);
                await jsonWriter.FlushAsync();
            }

            zipStream.Position = 0;

            using var requestContent = new MultipartFormDataContent
            {
                { new StreamContent(zipStream), nameof(ImportViewModel.Content), "Recipe.zip" },
                { new StringContent(RemoteDeploymentClientName), nameof(ImportViewModel.ClientName) },
                { new StringContent(RemoteDeploymentApiKey), nameof(ImportViewModel.ApiKey) },
            };

            var response = await Client.PostAsync("OrchardCore.Deployment.Remote/ImportRemoteInstance/Import", requestContent);
            if (ensureSuccess)
            {
                response.EnsureSuccessStatusCode();
            }

            return response;
        }
    }
}
