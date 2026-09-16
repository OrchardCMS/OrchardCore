using OrchardCore.Indexing.Core;
using OrchardCore.Indexing.Core.Operations;
using OrchardCore.Tests.Apis.Context;

namespace OrchardCore.Tests.Modules.OrchardCore.Indexing;

public class IndexOperationStoreTests
{
    [Fact]
    public async Task Operations_FreshScopes_PreserveStateAndRejectInvalidTransitions()
    {
        using var context = new SiteContext();
        await context.InitializeAsync();
        string operationId = null;
        await context.UsingTenantScopeAsync(async scope =>
        {
            var operation = await scope.ServiceProvider.GetRequiredService<IndexOperationStore>()
                .CreateAsync("index", IndexLifecycleAction.Rebuild);
            operationId = operation.OperationId;
            Assert.Equal(IndexOperationState.Pending, operation.State);
            Assert.Equal(32, operationId.Length);
        });
        await context.UsingTenantScopeAsync(async scope =>
        {
            var store = scope.ServiceProvider.GetRequiredService<IndexOperationStore>();
            var operation = await store.FindAsync(operationId);
            Assert.Equal("index", operation.IndexId);
            Assert.Equal(IndexLifecycleAction.Rebuild, operation.Action);
            Assert.True(await store.TransitionAsync(operationId, IndexOperationState.Pending, IndexOperationState.Running));
            Assert.False(await store.TransitionAsync(operationId, IndexOperationState.Pending, IndexOperationState.Running));
            await Assert.ThrowsAsync<InvalidOperationException>(() => store.TransitionAsync(operationId,
                IndexOperationState.Running, IndexOperationState.Completed));
            await Assert.ThrowsAsync<InvalidOperationException>(() => store.TransitionAsync(operationId,
                IndexOperationState.Running, IndexOperationState.Failed, new IndexProcessingResult { IndexId = "foreign" }));
            Assert.True(await store.TransitionAsync(operationId, IndexOperationState.Running, IndexOperationState.Failed));
        });
        await context.UsingTenantScopeAsync(async scope =>
        {
            var store = scope.ServiceProvider.GetRequiredService<IndexOperationStore>();
            var operation = await store.FindAsync(operationId);
            Assert.Equal(IndexOperationState.Failed, operation.State);
            Assert.True(operation.UpdatedUtc >= operation.CreatedUtc);
            await Assert.ThrowsAsync<InvalidOperationException>(() => store.TransitionAsync(operationId,
                IndexOperationState.Failed, IndexOperationState.Running));
        });
    }

    [Fact]
    public async Task Operations_AnotherTenant_CannotReadOrTransition()
    {
        using var owner = new SiteContext();
        using var other = new SiteContext();
        await owner.InitializeAsync();
        await other.InitializeAsync();
        string operationId = null;
        await owner.UsingTenantScopeAsync(async scope => operationId = (await scope.ServiceProvider
            .GetRequiredService<IndexOperationStore>().CreateAsync("same-index", IndexLifecycleAction.Reset)).OperationId);
        await other.UsingTenantScopeAsync(async scope =>
        {
            var store = scope.ServiceProvider.GetRequiredService<IndexOperationStore>();
            Assert.Null(await store.FindAsync(operationId));
            Assert.False(await store.TransitionAsync(operationId, IndexOperationState.Pending, IndexOperationState.Running));
        });
        await owner.UsingTenantScopeAsync(async scope => Assert.Equal(IndexOperationState.Pending,
            (await scope.ServiceProvider.GetRequiredService<IndexOperationStore>().FindAsync(operationId)).State));
    }
}
