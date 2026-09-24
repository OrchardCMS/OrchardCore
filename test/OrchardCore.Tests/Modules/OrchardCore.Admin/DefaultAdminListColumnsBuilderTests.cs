using OrchardCore.Admin;
using OrchardCore.Admin.Models;
using OrchardCore.Admin.Services;

namespace OrchardCore.Tests.Modules.OrchardCore.Admin;

public class DefaultAdminListColumnsBuilderTests
{
    [Fact]
    public async Task BuildAsync_NoProvider_ReturnsNoColumns()
    {
        var builder = CreateBuilder();

        Assert.Empty(await builder.BuildAsync("Contents", cancellationToken: TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task BuildAsync_ProvidersOfAList_OnlyAlterThatList()
    {
        var builder = CreateBuilder(new OwnerColumnProvider("Contents"), new CultureColumnProvider());

        var contents = await builder.BuildAsync("Contents", cancellationToken: TestContext.Current.CancellationToken);
        var users = await builder.BuildAsync("Users", cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(["Select", "Title", "Culture", "Actions"], contents.Select(c => c.Name));
        Assert.Empty(users);
    }

    [Fact]
    public async Task BuildAsync_PositionedColumns_AreSortedWhateverTheProviderOrder()
    {
        // Two features insert columns between the ones of the owner; the result does not depend on the order
        // they run in.
        var forward = await CreateBuilder(
                new OwnerColumnProvider("Contents"),
                new PositionedColumnProvider("Owner", "15"),
                new PositionedColumnProvider("Culture", "25"),
                new PositionedColumnProvider("Unpositioned", null))
            .BuildAsync("Contents", cancellationToken: TestContext.Current.CancellationToken);

        var backward = await CreateBuilder(
                new PositionedColumnProvider("Unpositioned", null),
                new PositionedColumnProvider("Culture", "25"),
                new PositionedColumnProvider("Owner", "15"),
                new OwnerColumnProvider("Contents"))
            .BuildAsync("Contents", cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(["Select", "Owner", "Title", "Culture", "Unpositioned", "Actions"], forward.Select(c => c.Name));
        Assert.Equal(forward.Select(c => c.Name), backward.Select(c => c.Name));
    }

    [Fact]
    public async Task BuildAsync_ProviderRunningAfterTheOwner_CanFindAndRemoveItsColumns()
    {
        var builder = CreateBuilder(new OwnerColumnProvider("Contents"), new RemovingColumnProvider("Title"));

        var columns = await builder.BuildAsync("Contents", cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(["Select", "Actions"], columns.Select(c => c.Name));
    }

    [Fact]
    public async Task BuildAsync_DataOfThePage_ReachesTheProviders()
    {
        var provider = new DataColumnProvider();
        var builder = CreateBuilder(provider);
        var data = new Dictionary<string, object>
        {
            ["ContentTypes"] = new[] { "BlogPost" },
            ["Count"] = 3,
        };

        var columns = await builder.BuildAsync("Contents", data, TestContext.Current.CancellationToken);

        // The provider added a column for the content type the page is filtered by.
        Assert.Equal(["BlogPost"], columns.Select(column => column.Name));

        // A key that is not there, or that holds another type, falls back instead of throwing.
        Assert.Equal(3, provider.Count);
        Assert.Null(provider.MissingKey);
        Assert.Null(provider.WrongType);
    }

    [Fact]
    public async Task BuildAsync_NoDataFromThePage_GivesProvidersEmptyData()
    {
        var provider = new DataColumnProvider();
        var builder = CreateBuilder(provider);

        var columns = await builder.BuildAsync("Contents", cancellationToken: TestContext.Current.CancellationToken);

        Assert.Empty(columns);
        Assert.Empty(provider.Data);
    }

    [Fact]
    public async Task BuildAsync_ProviderAddingAColumnTheListHas_IsLoggedAndTheOtherColumnsAreKept()
    {
        var logger = new ListLogger<DefaultAdminListColumnsBuilder>();
        var builder = new DefaultAdminListColumnsBuilder(
            [new OwnerColumnProvider("Contents"), new PositionedColumnProvider("title", "30"), new PositionedColumnProvider("Culture", "25")],
            logger);

        var columns = await builder.BuildAsync("Contents", cancellationToken: TestContext.Current.CancellationToken);

        // The names are compared ignoring case, so "title" is the Title column of the owner, which stays as it was.
        Assert.Equal(["Select", "Title", "Culture", "Actions"], columns.Select(c => c.Name));
        Assert.Equal("20", columns.Single(c => c.Name == "Title").Position);

        var error = Assert.Single(logger.Entries, entry => entry.Level == LogLevel.Error);
        Assert.IsType<ArgumentException>(error.Exception);
        Assert.Contains("already has a 'title' column", error.Exception.Message);
    }

    [Fact]
    public async Task BuildAsync_ColumnRenderingNoZone_IsLogged()
    {
        var logger = new ListLogger<DefaultAdminListColumnsBuilder>();
        var builder = new DefaultAdminListColumnsBuilder([new PositionedColumnProvider("Empty", "40", zones: [])], logger);

        var columns = await builder.BuildAsync("Contents", cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(["Empty"], columns.Select(c => c.Name));
        Assert.Contains(logger.Entries, entry => entry.Level == LogLevel.Warning && entry.Message.Contains("'Empty' column") && entry.Message.Contains("renders no zone"));
    }

    private static DefaultAdminListColumnsBuilder CreateBuilder(params IAdminListColumnProvider[] providers)
        => new(providers, NullLogger<DefaultAdminListColumnsBuilder>.Instance);

    // Declares the columns of its list, like the module owning it does.
    private sealed class OwnerColumnProvider(string listName) : IAdminListColumnProvider
    {
        public Task BuildAsync(AdminListColumnsContext context, CancellationToken cancellationToken = default)
        {
            if (context.ListName == listName)
            {
                context.Columns.Add(AdminListColumns.Select());
                context.Columns.Add(new AdminListColumn { Name = "Title", Position = "20", Zones = ["Title"] });
                context.Columns.Add(AdminListColumns.Actions(new LocalizedString("Actions", "Actions")));
            }

            return Task.CompletedTask;
        }
    }

    private sealed class CultureColumnProvider : IAdminListColumnProvider
    {
        public Task BuildAsync(AdminListColumnsContext context, CancellationToken cancellationToken = default)
        {
            if (context.ListName == "Contents")
            {
                context.Columns.Add(new AdminListColumn
                {
                    Name = "Culture",
                    Position = "25",
                    Title = new LocalizedString("Culture", "Culture"),
                    Zones = ["Culture"],
                });
            }

            return Task.CompletedTask;
        }
    }

    private sealed class PositionedColumnProvider(string name, string position, string[] zones = null) : IAdminListColumnProvider
    {
        public Task BuildAsync(AdminListColumnsContext context, CancellationToken cancellationToken = default)
        {
            context.Columns.Add(new AdminListColumn { Name = name, Position = position, Zones = zones ?? [name] });

            return Task.CompletedTask;
        }
    }

    private sealed class RemovingColumnProvider(string name) : IAdminListColumnProvider
    {
        public Task BuildAsync(AdminListColumnsContext context, CancellationToken cancellationToken = default)
        {
            Assert.NotNull(context.Find(name));
            Assert.True(context.Remove(name));
            Assert.Null(context.Find(name));

            return Task.CompletedTask;
        }
    }

    private sealed class DataColumnProvider : IAdminListColumnProvider
    {
        public IReadOnlyDictionary<string, object> Data { get; private set; }

        public int Count { get; private set; }

        public string MissingKey { get; private set; }

        public string WrongType { get; private set; }

        public Task BuildAsync(AdminListColumnsContext context, CancellationToken cancellationToken = default)
        {
            Data = context.Data;
            Count = context.GetData<int>("Count");
            MissingKey = context.GetData<string>("NotThere");
            WrongType = context.GetData<string>("Count");

            if (context.TryGetData<string[]>("ContentTypes", out var contentTypes))
            {
                foreach (var contentType in contentTypes)
                {
                    context.Columns.Add(new AdminListColumn { Name = contentType, Zones = [contentType] });
                }
            }

            return Task.CompletedTask;
        }
    }

    private sealed class ListLogger<T> : ILogger<T>
    {
        public ConcurrentBag<(LogLevel Level, string Message, Exception Exception)> Entries { get; } = [];

        public IDisposable BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            => Entries.Add((logLevel, formatter(state, exception), exception));
    }
}
