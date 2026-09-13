using System.Text.Json.Nodes;
using Microsoft.Extensions.Logging.Abstractions;
using OrchardCore.Indexing;
using OrchardCore.Indexing.Core;
using OrchardCore.Indexing.Core.Handlers;
using OrchardCore.Indexing.Models;
using OrchardCore.Infrastructure.Entities;

namespace OrchardCore.Tests.Modules.OrchardCore.Indexing;

public class IndexHandlerFailureTests
{
    [Theory]
    [InlineData("update")]
    [InlineData("validate")]
    public async Task Update_HandlerFailure_DoesNotSaveAndRestoresTrackedProfile(string phase)
    {
        var profile = new IndexProfile { Id = "id", Name = "Original" };
        profile.Properties["Extension"] = new JsonObject { ["Value"] = "Original" };
        var store = new Mock<IIndexProfileStore>();
        var manager = new DefaultIndexProfileManager(store.Object, [new FailingHandler(phase)], NullLogger<DefaultIndexProfileManager>.Instance);

        await Assert.ThrowsAsync<InvalidOperationException>(() => manager.UpdateAsync(profile).AsTask());

        Assert.Equal("Original", profile.Name);
        Assert.Equal("Original", profile.Properties["Extension"]["Value"].GetValue<string>());
        store.Verify(value => value.UpdateAsync(It.IsAny<IndexProfile>()), Times.Never);
    }

    [Fact]
    public async Task Create_HandlerFailure_DoesNotPersist()
    {
        var store = new Mock<IIndexProfileStore>();
        var manager = new DefaultIndexProfileManager(store.Object, [new FailingHandler("create")], NullLogger<DefaultIndexProfileManager>.Instance);

        await Assert.ThrowsAsync<InvalidOperationException>(() => manager.CreateAsync(new IndexProfile()).AsTask());

        store.Verify(value => value.CreateAsync(It.IsAny<IndexProfile>()), Times.Never);
    }

    [Fact]
    public async Task New_HandlerFailure_DoesNotReturnAnIncompleteProfile()
    {
        var store = new Mock<IIndexProfileStore>(MockBehavior.Strict);
        var manager = new DefaultIndexProfileManager(store.Object, [new FailingHandler("initialize")], NullLogger<DefaultIndexProfileManager>.Instance);

        await Assert.ThrowsAsync<InvalidOperationException>(() => manager.NewAsync("Lucene", "Content").AsTask());

        store.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData("created")]
    [InlineData("updated")]
    [InlineData("deleted")]
    public async Task PostPersistenceFailure_PropagatesWithoutClaimingRollback(string phase)
    {
        var profile = new IndexProfile { Id = "id", Name = "Original" };
        profile.Properties["Extension"] = new JsonObject { ["Value"] = "Original" };
        var store = new Mock<IIndexProfileStore>();
        store.Setup(value => value.DeleteAsync(profile)).ReturnsAsync(true);
        var manager = new DefaultIndexProfileManager(store.Object, [new FailingHandler(phase)], NullLogger<DefaultIndexProfileManager>.Instance);

        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            if (phase == "created") { await manager.CreateAsync(profile); }
            else if (phase == "updated") { await manager.UpdateAsync(profile); }
            else { await manager.DeleteAsync(profile); }
        });

        if (phase == "created") { store.Verify(value => value.CreateAsync(profile), Times.Once); }
        else if (phase == "updated")
        {
            store.Verify(value => value.UpdateAsync(profile), Times.Once);
            Assert.Equal("Changed", profile.Name);
        }
        else { store.Verify(value => value.DeleteAsync(profile), Times.Once); }
    }

    [Fact]
    public async Task Reset_HandlerFailure_Propagates()
    {
        var store = new Mock<IIndexProfileStore>(MockBehavior.Strict);
        var manager = new DefaultIndexProfileManager(store.Object, [new FailingHandler("reset")], NullLogger<DefaultIndexProfileManager>.Instance);
        await Assert.ThrowsAsync<InvalidOperationException>(() => manager.ResetAsync(new IndexProfile()).AsTask());
        store.VerifyNoOtherCalls();
    }

    private sealed class FailingHandler : IndexProfileHandlerBase
    {
        private readonly string _phase;

        public FailingHandler(string phase)
        {
            _phase = phase;
        }

        public override Task InitializingAsync(InitializingContext<IndexProfile> context) => Fail("initialize");
        public override Task ResetAsync(IndexProfileResetContext context) => Fail("reset");
        public override Task CreatingAsync(CreatingContext<IndexProfile> context) => Fail("create");
        public override Task CreatedAsync(CreatedContext<IndexProfile> context) => Fail("created");
        public override Task UpdatedAsync(UpdatedContext<IndexProfile> context) => Fail("updated");
        public override Task DeletedAsync(DeletedContext<IndexProfile> context) => Fail("deleted");
        public override Task ValidatingAsync(ValidatingContext<IndexProfile> context) => Fail("validate");
        public override Task UpdatingAsync(UpdatingContext<IndexProfile> context)
        {
            context.Model.Name = "Changed";
            context.Model.Properties["Extension"]["Value"] = "Changed";
            return Fail("update");
        }

        private Task Fail(string current) => _phase == current
            ? Task.FromException(new InvalidOperationException("Index handler failed.")) : Task.CompletedTask;
    }
}
