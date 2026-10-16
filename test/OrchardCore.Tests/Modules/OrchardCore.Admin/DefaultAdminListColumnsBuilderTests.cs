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
        using var services = CreateServices(provider.Object);
        var builder = new DefaultAdminListColumnsBuilder(services, NullLogger<DefaultAdminListColumnsBuilder>.Instance);

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
        using var services = CreateServices();
        var builder = new DefaultAdminListColumnsBuilder(services, NullLogger<DefaultAdminListColumnsBuilder>.Instance);
        var columns = await builder.BuildColumnsAsync("Contents",
            [new() { Name = "Title", Position = "20" }, AdminListColumns.Select()],
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(["Select", "Title"], columns.Columns.Select(c => c.Name));
        Assert.Empty(columns.Data);
    }

    [Fact]
    public void AddAdminListColumnsProvider_RepeatedRegistration_IsIdempotent()
    {
        var services = new ServiceCollection()
            .AddAdminListColumnsProvider<TestColumnsProvider>("Contents")
            .AddAdminListColumnsProvider<TestColumnsProvider>("Contents");

        Assert.Single(services);
        Assert.Equal(typeof(IAdminListColumnsProvider), services[0].ServiceType);
        Assert.Equal(new AdminListColumnsProviderKey("Contents"), services[0].ServiceKey);
    }

    [Fact]
    public async Task BuildColumnsAsync_ProviderFailure_IsLoggedAndOtherProvidersContinue()
    {
        var failing = new Mock<IAdminListColumnsProvider>();
        failing.Setup(p => p.PopulateAsync(It.IsAny<AdminListColumnsContext>()))
            .ThrowsAsync(new InvalidOperationException("Invalid column"));
        var logger = new Mock<ILogger<DefaultAdminListColumnsBuilder>>();
        using var services = new ServiceCollection()
            .AddKeyedSingleton(new AdminListColumnsProviderKey("Contents"), failing.Object)
            .AddKeyedSingleton<IAdminListColumnsProvider>(new AdminListColumnsProviderKey("Contents"), new TestColumnsProvider())
            .BuildServiceProvider();
        var builder = new DefaultAdminListColumnsBuilder(services, logger.Object);

        var columns = await builder.BuildColumnsAsync("Contents", [],
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal("Culture", Assert.Single(columns.Columns).Name);
        logger.Verify(l => l.Log(
            LogLevel.Error, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(),
            It.IsAny<InvalidOperationException>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);
    }

    [Fact]
    public async Task BuildColumnsAsync_ListKey_OnlyCreatesAndRunsMatchingProvidersInRegistrationOrder()
    {
        var created = new List<string>();
        var services = new ServiceCollection()
            .AddAdminListColumnsProvider<TestColumnsProvider>("Contents")
            .AddAdminListColumnsProvider<TestColumnsProvider>("Users")
            .AddKeyedScoped<IAdminListColumnsProvider>(new AdminListColumnsProviderKey("Contents"), (_, _) =>
            {
                created.Add("First");
                return CreateProvider("First");
            })
            .AddKeyedScoped<IAdminListColumnsProvider>(new AdminListColumnsProviderKey("Contents"), (_, _) =>
            {
                created.Add("Second");
                return CreateProvider("Second");
            })
            .AddKeyedScoped<IAdminListColumnsProvider>(new AdminListColumnsProviderKey("Users"), (_, _) =>
                throw new InvalidOperationException("Unrelated provider must not be constructed"))
            .AddKeyedScoped<IAdminListColumnsProvider>("Contents", (_, _) =>
                throw new InvalidOperationException("String key must not match"));
        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        var builder = new DefaultAdminListColumnsBuilder(scope.ServiceProvider, NullLogger<DefaultAdminListColumnsBuilder>.Instance);

        var columns = await builder.BuildColumnsAsync("Contents", [], cancellationToken: TestContext.Current.CancellationToken);
        var unrelated = await builder.BuildColumnsAsync("Roles", [], cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(["First", "Second"], created);
        Assert.Equal(["Culture", "First", "Second"], columns.Columns.Select(column => column.Name));
        Assert.Empty(unrelated.Columns);
        Assert.Empty(scope.ServiceProvider.GetServices<IAdminListColumnsProvider>());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void AddAdminListColumnsProvider_InvalidName_Throws(string name)
        => Assert.ThrowsAny<ArgumentException>(() => new ServiceCollection().AddAdminListColumnsProvider<TestColumnsProvider>(name));

    [Fact]
    public void AddAdminListColumnsProvider_MultipleLists_PreservesScopedLifetimeAndEachRegistration()
    {
        using var provider = new ServiceCollection()
            .AddAdminListColumnsProvider<TestColumnsProvider>("Contents")
            .AddAdminListColumnsProvider<TestColumnsProvider>("Users")
            .BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true });
        using var first = provider.CreateScope();
        using var second = provider.CreateScope();
        var key = new AdminListColumnsProviderKey("Contents");
        var contents = Assert.Single(first.ServiceProvider.GetKeyedServices<IAdminListColumnsProvider>(key));

        Assert.Same(contents, Assert.Single(first.ServiceProvider.GetKeyedServices<IAdminListColumnsProvider>(key)));
        Assert.NotSame(contents, Assert.Single(second.ServiceProvider.GetKeyedServices<IAdminListColumnsProvider>(key)));
        Assert.NotSame(contents, Assert.Single(first.ServiceProvider.GetKeyedServices<IAdminListColumnsProvider>(new AdminListColumnsProviderKey("Users"))));
        Assert.Empty(first.ServiceProvider.GetKeyedServices<IAdminListColumnsProvider>(new AdminListColumnsProviderKey("contents")));
    }

    private static IAdminListColumnsProvider CreateProvider(string name)
    {
        var provider = new Mock<IAdminListColumnsProvider>();
        provider.Setup(value => value.PopulateAsync(It.IsAny<AdminListColumnsContext>()))
            .Callback<AdminListColumnsContext>(context => context.Columns.Add(new AdminListColumn { Name = name }))
            .Returns(Task.CompletedTask);
        return provider.Object;
    }

    private static ServiceProvider CreateServices(params IAdminListColumnsProvider[] providers)
    {
        var services = new ServiceCollection();
        foreach (var provider in providers)
        {
            services.AddKeyedSingleton(new AdminListColumnsProviderKey("Contents"), provider);
        }

        return services.BuildServiceProvider();
    }

    private sealed class TestColumnsProvider : IAdminListColumnsProvider
    {
        public Task PopulateAsync(AdminListColumnsContext context)
        {
            context.Columns.Add(new AdminListColumn { Name = "Culture" });
            return Task.CompletedTask;
        }
    }
}
