using OrchardCore.Admin;
using OrchardCore.Admin.Models;
using OrchardCore.Admin.Services;

namespace OrchardCore.Tests.Modules.OrchardCore.Admin;

public class DefaultAdminListColumnsBuilderTests
{
    [Fact]
    public async Task BuildColumnsAsync_Providers_ExtendViewColumnsAndSortByPosition()
    {
        var provider = new Mock<IAdminListColumnsProvider>();
        provider.Setup(p => p.PopulateAsync(It.IsAny<AdminListColumnsContext>()))
            .Callback<AdminListColumnsContext>(context =>
            {
                Assert.Equal("Contents", context.ListName);
                Assert.Equal("BlogPost", Assert.Single(context.GetData<string[]>("ContentTypes")));
                context.Columns.Add(new AdminListColumn { Name = "Culture", Position = "25" });
                context.Find("Title").Width = "50%";
                context.Remove("Removed");
            })
            .Returns(Task.CompletedTask);
        var builder = CreateBuilder(provider.Object);

        var columns = await builder.BuildColumnsAsync("Contents",
            [
                AdminListColumns.Actions(new("Actions", "Actions")),
                new() { Name = "Title", Position = "20" },
                new() { Name = "Removed", Position = "30" },
                AdminListColumns.Select(),
            ],
            new Dictionary<string, object> { ["ContentTypes"] = new[] { "BlogPost" } },
            TestContext.Current.CancellationToken);

        Assert.Equal(["Select", "Title", "Culture", "Actions"], columns.Columns.Select(c => c.Name));
        Assert.Equal("50%", columns.Find("Title").Width);
        Assert.Null(columns.GetData<string>("Missing"));
        Assert.False(columns.TryGetData<string>("ContentTypes", out _));
    }

    [Fact]
    public async Task BuildColumnsAsync_NoRowsOrProviders_KeepsViewColumns()
    {
        var columns = await CreateBuilder().BuildColumnsAsync("Contents",
            [new() { Name = "Title", Position = "20" }, AdminListColumns.Select()],
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(["Select", "Title"], columns.Columns.Select(c => c.Name));
        Assert.Empty(columns.Data);
    }

    [Fact]
    public void AddAdminListColumnsProvider_RepeatedRegistration_IsIdempotent()
    {
        var services = new ServiceCollection()
            .AddAdminListColumnsProvider<TestColumnsProvider>()
            .AddAdminListColumnsProvider<TestColumnsProvider>();

        Assert.Single(services);
        Assert.Equal(typeof(IAdminListColumnsProvider), services[0].ServiceType);
    }

    [Fact]
    public async Task BuildColumnsAsync_ProviderFailure_IsLoggedAndOtherProvidersContinue()
    {
        var failing = new Mock<IAdminListColumnsProvider>();
        failing.Setup(p => p.PopulateAsync(It.IsAny<AdminListColumnsContext>()))
            .ThrowsAsync(new InvalidOperationException("Invalid column"));
        var logger = new Mock<ILogger<DefaultAdminListColumnsBuilder>>();
        var builder = new DefaultAdminListColumnsBuilder(
            [failing.Object, new TestColumnsProvider()], logger.Object);

        var columns = await builder.BuildColumnsAsync("Contents", [],
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal("Culture", Assert.Single(columns.Columns).Name);
        logger.Verify(l => l.Log(
            LogLevel.Error, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(),
            It.IsAny<InvalidOperationException>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);
    }

    private static DefaultAdminListColumnsBuilder CreateBuilder(params IAdminListColumnsProvider[] providers)
        => new(providers, NullLogger<DefaultAdminListColumnsBuilder>.Instance);

    private sealed class TestColumnsProvider : IAdminListColumnsProvider
    {
        public Task PopulateAsync(AdminListColumnsContext context)
        {
            context.Columns.Add(new AdminListColumn { Name = "Culture" });
            return Task.CompletedTask;
        }
    }
}
