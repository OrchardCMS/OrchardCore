using System.Text;
using System.Text.Json.Nodes;
using OrchardCore.ContentManagement;
using OrchardCore.Tests.Apis.Context;

namespace OrchardCore.Tests.Apis.ContentManagement.ContentApiController;

public class ContentPayloadValidationTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Update_ExplicitNull_ClearsValueAndPreservesOmittedProperties(bool draft)
    {
        using var context = new BlogPostApiControllerContext();
        await context.InitializeAsync();
        var id = context.BlogPost.ContentItemId;
        var original = JsonNode.Parse(await context.Client.GetStringAsync($"api/content/{id}", TestContext.Current.CancellationToken));
        Assert.NotNull(original["MarkdownBodyPart"]?["Markdown"]);
        const string payload = "{\"MarkdownBodyPart\":{\"Markdown\":null}}";

        // Validation must accept the same merged payload without changing the stored item.
        using var validationBody = new StringContent(payload, Encoding.UTF8, "application/json");
        using var validation = await context.Client.PostAsync($"api/content/{id}/validate", validationBody, TestContext.Current.CancellationToken);
        validation.EnsureSuccessStatusCode();
        var validated = JsonNode.Parse(await validation.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        Assert.True(validated["isValid"].GetValue<bool>());
        var unchanged = JsonNode.Parse(await context.Client.GetStringAsync($"api/content/{id}", TestContext.Current.CancellationToken));
        Assert.Equal(original["MarkdownBodyPart"]?["Markdown"]?.ToJsonString(), unchanged["MarkdownBodyPart"]?["Markdown"]?.ToJsonString());

        var route = $"api/content/{id}" + (draft ? "/draft" : string.Empty);
        for (var attempt = 0; attempt < 2; attempt++)
        {
            using var updateBody = new StringContent(payload, Encoding.UTF8, "application/json");
            using var response = await context.Client.PutAsync(route, updateBody, TestContext.Current.CancellationToken);
            response.EnsureSuccessStatusCode();
            var result = JsonNode.Parse(await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
            Assert.Null(result["MarkdownBodyPart"]?["Markdown"]);
            Assert.Equal(original["TitlePart"]?.ToJsonString(), result["TitlePart"]?.ToJsonString());
            Assert.Equal(original["Owner"]?.ToJsonString(), result["Owner"]?.ToJsonString());
        }

        var saved = JsonNode.Parse(await context.Client.GetStringAsync($"api/content/{id}?version=latest", TestContext.Current.CancellationToken));
        Assert.Null(saved["MarkdownBodyPart"]?["Markdown"]);
        Assert.Equal(original["TitlePart"]?.ToJsonString(), saved["TitlePart"]?.ToJsonString());
    }

    [Theory]
    [InlineData("{\"TitlePart\":{\"Title\":{\"a\":\"b\"}}}", "TitlePart.Title")]
    [InlineData("{\"TitlePart\":{\"Title\":[\"wrong\"]}}", "TitlePart.Title")]
    [InlineData("{\"TitlePart\":{\"Title\":42}}", "TitlePart.Title")]
    [InlineData("{\"TitlePart\":\"not a part\"}", "TitlePart")]
    [InlineData("{\"MarkdownBodyPart\":{\"Markdown\":true}}", "MarkdownBodyPart.Markdown")]
    [InlineData("{\"DisplayText\":{\"a\":\"b\"}}", "DisplayText")]
    public async Task InvalidPayload_ValidateAndSave_RejectWithoutCreatingDraft(string json, string errorPath)
    {
        using var context = new BlogPostApiControllerContext();
        await context.InitializeAsync();
        var id = context.BlogPost.ContentItemId;
        var originalVersion = context.BlogPost.ContentItemVersionId;
        foreach (var (method, route) in new[]
        {
            (HttpMethod.Post, $"api/content/{id}/validate"),
            (HttpMethod.Put, $"api/content/{id}/draft"),
            (HttpMethod.Put, $"api/content/{id}"),
        })
        {
            await AssertInvalidAsync(context.Client, method, route, json, errorPath);
        }

        var create = JsonNode.Parse(json).AsObject();
        create["ContentType"] = "BlogPost";
        foreach (var route in new[] { "api/content/validate", "api/content/draft", "api/content" })
        {
            await AssertInvalidAsync(context.Client, HttpMethod.Post, route, create.ToJsonString(), errorPath);
        }

        // The save endpoint with a body ID must use the same validation path.
        create["ContentItemId"] = id;
        await AssertInvalidAsync(context.Client, HttpMethod.Post, "api/content?draft=true", create.ToJsonString(), errorPath);

        await context.UsingTenantScopeAsync(async scope =>
        {
            var manager = scope.ServiceProvider.GetRequiredService<IContentManager>();
            Assert.Null(await manager.GetAsync(id, VersionOptions.Draft));
            Assert.Equal(originalVersion, (await manager.GetAsync(id, VersionOptions.Published)).ContentItemVersionId);
            Assert.Single(await manager.GetAllVersionsAsync(id));
        });
    }

    [Fact]
    public async Task Validate_MissingTypeOrMismatchedId_ReturnsSpecificErrors()
    {
        using var context = new BlogPostApiControllerContext();
        await context.InitializeAsync();
        await AssertInvalidAsync(context.Client, HttpMethod.Post, "api/content/validate", "{\"TitlePart\":{\"Title\":\"Hello\"}}", "ContentType");
        await AssertInvalidAsync(context.Client, HttpMethod.Post, "api/content/draft", "{}", "ContentType");
        await AssertInvalidAsync(context.Client, HttpMethod.Post, $"api/content/{context.BlogPost.ContentItemId}/validate", "{\"ContentItemId\":\"wrong-id\"}", "ContentItemId");
    }

    [Fact]
    public async Task Validate_ValidPartialUpdate_SucceedsWithoutSaving()
    {
        using var context = new BlogPostApiControllerContext();
        await context.InitializeAsync();
        using var body = new StringContent("{\"TitlePart\":{\"Title\":\"Revised title\"},\"MarkdownBodyPart\":{\"Markdown\":\"**Valid** text\"}}", Encoding.UTF8, "application/json");
        var response = await context.Client.PostAsync($"api/content/{context.BlogPost.ContentItemId}/validate", body, TestContext.Current.CancellationToken);
        response.EnsureSuccessStatusCode();
        var json = JsonNode.Parse(await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        Assert.True(json["isValid"].GetValue<bool>());
        await context.UsingTenantScopeAsync(async scope =>
        {
            var manager = scope.ServiceProvider.GetRequiredService<IContentManager>();
            Assert.Null(await manager.GetAsync(context.BlogPost.ContentItemId, VersionOptions.Draft));
            Assert.Equal(context.BlogPost.DisplayText, (await manager.GetAsync(context.BlogPost.ContentItemId)).DisplayText);
        });
    }

    [Fact]
    public async Task InvalidStoredData_RejectsValidationUpdateAndRestore()
    {
        using var context = new BlogPostApiControllerContext();
        await context.InitializeAsync();
        var id = context.BlogPost.ContentItemId;
        await context.UsingTenantScopeAsync(async scope =>
        {
            var manager = scope.ServiceProvider.GetRequiredService<IContentManager>();
            var item = await manager.GetAsync(id);
            item.Merge(new { TitlePart = new { Title = new { a = "b" } } });
            await scope.ServiceProvider.GetRequiredService<YesSql.ISession>().SaveAsync(item);
        });

        await AssertInvalidAsync(context.Client, HttpMethod.Post, $"api/content/{id}/validate", "{}", "TitlePart.Title");
        await AssertInvalidAsync(context.Client, HttpMethod.Put, $"api/content/{id}/draft", "{}", "TitlePart.Title");
        await AssertInvalidAsync(context.Client, HttpMethod.Post, $"api/content/versions/{context.BlogPost.ContentItemVersionId}/restore", "{}", "TitlePart.Title");

    }

    private static async Task AssertInvalidAsync(HttpClient client, HttpMethod method, string route, string json, string path)
    {
        using var request = new HttpRequestMessage(method, route) { Content = new StringContent(json, Encoding.UTF8, "application/json") };
        using var response = await client.SendAsync(request, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = JsonNode.Parse(await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        Assert.NotNull(problem["errors"]?[path]);
    }
}
