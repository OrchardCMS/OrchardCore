using OrchardCore.ContentManagement;
using OrchardCore.Contents;
using OrchardCore.Contents.Models;
using OrchardCore.RemoteManagement;
using OrchardCore.Tests.Apis.Context;

namespace OrchardCore.Tests.Apis.ContentManagement.ContentApiController;

public class ContentVersionApiTests
{
    [Fact]
    public async Task Versions_ListReadRestoreAndDelete_PreserveSourceAndPublishedContent()
    {
        using var context = new BlogPostApiControllerContext();
        await context.InitializeAsync();
        var original = context.BlogPost;
        var id = original.ContentItemId;
        var firstVersion = original.ContentItemVersionId;
        var draftResponse = await context.Client.PostAsync($"api/content/{id}/draft", null, TestContext.Current.CancellationToken);
        draftResponse.EnsureSuccessStatusCode();
        var draft = await draftResponse.Content.ReadAsAsync<ContentItem>();
        Assert.NotEqual(firstVersion, draft.ContentItemVersionId);
        var conflict = await context.Client.DeleteAsync($"api/content/versions/{draft.ContentItemVersionId}", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Conflict, conflict.StatusCode);
        using var update = new StringContent("{\"Owner\":\"current-owner\"}", System.Text.Encoding.UTF8, "application/json");
        (await context.Client.PutAsync($"api/content/{id}/draft", update, TestContext.Current.CancellationToken)).EnsureSuccessStatusCode();
        (await context.Client.PostAsync($"api/content/{id}/publish", null, TestContext.Current.CancellationToken)).EnsureSuccessStatusCode();

        var list = await (await context.Client.GetAsync($"api/content/{id}/versions?skip=0&take=1", TestContext.Current.CancellationToken)).Content.ReadAsAsync<ContentItemsResponse>();
        Assert.Equal(2, list.TotalCount);
        Assert.Equal(draft.ContentItemVersionId, Assert.Single(list.Items).ContentItemVersionId);
        var oldResponse = await context.Client.GetAsync($"api/content/versions/{firstVersion}", TestContext.Current.CancellationToken);
        oldResponse.EnsureSuccessStatusCode();
        var old = await oldResponse.Content.ReadAsAsync<ContentItem>();
        Assert.False(old.Latest);
        Assert.False(old.Published);

        var renderedResponse = await context.Client.GetAsync($"api/content/versions/{firstVersion}/render", TestContext.Current.CancellationToken);
        renderedResponse.EnsureSuccessStatusCode();
        var rendered = await renderedResponse.Content.ReadAsAsync<ContentItemRenderResponse>();
        Assert.Equal(firstVersion, rendered.ContentItemVersionId);
        Assert.False(string.IsNullOrWhiteSpace(rendered.Html));

        var restoredResponse = await context.Client.PostAsync($"api/content/versions/{firstVersion}/restore", null, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Created, restoredResponse.StatusCode);
        var restored = await restoredResponse.Content.ReadAsAsync<ContentItem>();
        Assert.NotEqual(firstVersion, restored.ContentItemVersionId);
        Assert.NotEqual(draft.ContentItemVersionId, restored.ContentItemVersionId);
        Assert.True(restored.Latest);
        Assert.False(restored.Published);
        Assert.Equal(original.ContentItemId, restored.ContentItemId);
        Assert.Equal(original.DisplayText, restored.DisplayText);
        Assert.Equal("current-owner", restored.Owner);
        var repeated = await context.Client.PostAsync($"api/content/versions/{firstVersion}/restore", null, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Conflict, repeated.StatusCode);
        var replaceResponse = await context.Client.PostAsync($"api/content/versions/{firstVersion}/restore?replaceDraft=true", null, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Created, replaceResponse.StatusCode);
        var replaced = await replaceResponse.Content.ReadAsAsync<ContentItem>();
        Assert.NotEqual(restored.ContentItemVersionId, replaced.ContentItemVersionId);

        var unchanged = await (await context.Client.GetAsync($"api/content/versions/{firstVersion}", TestContext.Current.CancellationToken)).Content.ReadAsAsync<ContentItem>();
        Assert.Equal(old.ContentItemVersionId, unchanged.ContentItemVersionId);
        Assert.False(unchanged.Latest);
        Assert.False(unchanged.Published);
        Assert.Equal(old.Content.ToString(), unchanged.Content.ToString());
        var published = await (await context.Client.GetAsync($"api/content/{id}", TestContext.Current.CancellationToken)).Content.ReadAsAsync<ContentItem>();
        Assert.Equal(draft.ContentItemVersionId, published.ContentItemVersionId);
        Assert.Equal(HttpStatusCode.Conflict, (await context.Client.DeleteAsync($"api/content/versions/{draft.ContentItemVersionId}", TestContext.Current.CancellationToken)).StatusCode);

        Assert.Equal(HttpStatusCode.OK, (await context.Client.DeleteAsync($"api/content/versions/{firstVersion}", TestContext.Current.CancellationToken)).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await context.Client.GetAsync($"api/content/versions/{firstVersion}", TestContext.Current.CancellationToken)).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, (await context.Client.DeleteAsync($"api/content/versions/{firstVersion}", TestContext.Current.CancellationToken)).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await context.Client.PostAsync($"api/content/versions/{firstVersion}/restore", null, TestContext.Current.CancellationToken)).StatusCode);
        var final = await (await context.Client.GetAsync($"api/content/{id}?version=latest", TestContext.Current.CancellationToken)).Content.ReadAsAsync<ContentItem>();
        Assert.Equal(replaced.ContentItemVersionId, final.ContentItemVersionId);
    }

    [Fact]
    public async Task Versions_Authorization_HidesHistoryWithoutPreviewAndRejectsMutations()
    {
        var permissions = new PermissionsContext();
        using var context = new BlogPostApiControllerContext { PermissionsContext = permissions };
        await context.InitializeAsync();
        var id = context.BlogPost.ContentItemId;
        var archivedId = context.BlogPost.ContentItemVersionId;
        (await context.Client.PostAsync($"api/content/{id}/draft", null, TestContext.Current.CancellationToken)).EnsureSuccessStatusCode();
        var publish = await context.Client.PostAsync($"api/content/{id}/publish", null, TestContext.Current.CancellationToken);
        publish.EnsureSuccessStatusCode();
        var published = await publish.Content.ReadAsAsync<ContentItem>();

        permissions.UsePermissionsContext = true;
        permissions.AuthorizedPermissions = [RemoteManagementPermissions.AccessRemoteManagement, CommonPermissions.AccessContentApi, CommonPermissions.ListContent, CommonPermissions.ViewContent];
        Assert.Equal(HttpStatusCode.OK, (await context.Client.GetAsync($"api/content/versions/{published.ContentItemVersionId}", TestContext.Current.CancellationToken)).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await context.Client.GetAsync($"api/content/versions/{archivedId}", TestContext.Current.CancellationToken)).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await context.Client.GetAsync($"api/content/versions/{archivedId}/render", TestContext.Current.CancellationToken)).StatusCode);
        var list = await (await context.Client.GetAsync($"api/content/{id}/versions", TestContext.Current.CancellationToken)).Content.ReadAsAsync<ContentItemsResponse>();
        Assert.Equal(1, list.TotalCount);
        Assert.Equal(published.ContentItemVersionId, Assert.Single(list.Items).ContentItemVersionId);
        Assert.Equal(HttpStatusCode.Forbidden, (await context.Client.DeleteAsync($"api/content/versions/{archivedId}", TestContext.Current.CancellationToken)).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await context.Client.PostAsync($"api/content/versions/{archivedId}/restore", null, TestContext.Current.CancellationToken)).StatusCode);
        // Read access alone must not grant purge or restore permission.
        permissions.AuthorizedPermissions = [RemoteManagementPermissions.AccessRemoteManagement, CommonPermissions.AccessContentApi, CommonPermissions.PreviewContent];
        Assert.Equal(HttpStatusCode.OK, (await context.Client.GetAsync($"api/content/versions/{archivedId}", TestContext.Current.CancellationToken)).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await context.Client.DeleteAsync($"api/content/versions/{archivedId}", TestContext.Current.CancellationToken)).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await context.Client.PostAsync($"api/content/versions/{archivedId}/restore", null, TestContext.Current.CancellationToken)).StatusCode);
        permissions.AuthorizedPermissions = [RemoteManagementPermissions.AccessRemoteManagement];
        Assert.Equal(HttpStatusCode.Forbidden, (await context.Client.GetAsync($"api/content/versions/{published.ContentItemVersionId}", TestContext.Current.CancellationToken)).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await context.Client.DeleteAsync("api/content/versions/nonexistent", TestContext.Current.CancellationToken)).StatusCode);
    }
}
