using OrchardCore.Admin;
using OrchardCore.Admin.Models;
using OrchardCore.Admin.Services;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Implementation;
using OrchardCore.DisplayManagement.Layout;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Shapes;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.DisplayManagement.Zones;

namespace OrchardCore.Tests.Modules.OrchardCore.Admin;

public class DefaultAdminListColumnsBuilderTests
{
    [Fact]
    public async Task BuildColumnsAsync_NoDriver_ReturnsNoColumns()
    {
        var builder = CreateBuilder(services => { });

        var context = await builder.BuildColumnsAsync<Item>("Items", cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal("Items", context.ListName);
        Assert.Empty(context.Columns);
    }

    [Fact]
    public async Task BuildColumnsAsync_DriversOfOtherItems_AreNotCreated()
    {
        CountingColumnDriver.Created = 0;

        var builder = CreateBuilder(services => services.AddAdminListColumnDriver<Item, CountingColumnDriver>());

        // A list of other items does not create the drivers of these.
        var others = await builder.BuildColumnsAsync<OtherItem>("Others", cancellationToken: TestContext.Current.CancellationToken);
        var createdForOthers = CountingColumnDriver.Created;
        var items = await builder.BuildColumnsAsync<Item>("Items", cancellationToken: TestContext.Current.CancellationToken);

        Assert.Empty(others.Columns);
        Assert.Equal(0, createdForOthers);
        Assert.Equal(["Counted"], items.Columns.Select(c => c.Name));
    }

    [Fact]
    public async Task BuildColumnsAsync_DriverOfAnotherList_DoesNotAlterTheList()
    {
        // Both lists render the same items, e.g. the content items of the Contents list and of a list part.
        var builder = CreateBuilder(services => services
            .AddAdminListColumnDriver<Item, OwnerColumnDriver>()
            .AddAdminListColumnDriver<Item, CultureColumnDriver>());

        var items = await builder.BuildColumnsAsync<Item>("Items", cancellationToken: TestContext.Current.CancellationToken);
        var archive = await builder.BuildColumnsAsync<Item>("Archive", cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(["Select", "Title", "Culture", "Actions"], items.Columns.Select(c => c.Name));
        Assert.Equal(["Culture"], archive.Columns.Select(c => c.Name));
    }

    [Fact]
    public async Task BuildColumnsAsync_PositionedColumns_AreSortedWhateverTheDriverOrder()
    {
        // Two features insert columns between the ones of the owner; the result does not depend on the order
        // they run in.
        var forward = await CreateBuilder(
                new OwnerColumnDriver(),
                new PositionedColumnDriver("Owner", "15"),
                new PositionedColumnDriver("Culture", "25"),
                new PositionedColumnDriver("Unpositioned", null))
            .BuildColumnsAsync<Item>("Items", cancellationToken: TestContext.Current.CancellationToken);

        var backward = await CreateBuilder(
                new PositionedColumnDriver("Unpositioned", null),
                new PositionedColumnDriver("Culture", "25"),
                new PositionedColumnDriver("Owner", "15"),
                new OwnerColumnDriver())
            .BuildColumnsAsync<Item>("Items", cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(["Select", "Owner", "Title", "Culture", "Unpositioned", "Actions"], forward.Columns.Select(c => c.Name));
        Assert.Equal(forward.Columns.Select(c => c.Name), backward.Columns.Select(c => c.Name));
    }

    [Fact]
    public async Task BuildColumnsAsync_DriverRunningAfterTheOwner_CanFindAndRemoveItsColumns()
    {
        var builder = CreateBuilder(new OwnerColumnDriver(), new RemovingColumnDriver("Title"));

        var context = await builder.BuildColumnsAsync<Item>("Items", cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(["Select", "Actions"], context.Columns.Select(c => c.Name));
    }

    [Fact]
    public async Task BuildColumnsAsync_DataOfThePage_ReachesTheDrivers()
    {
        var driver = new DataColumnDriver();
        var builder = CreateBuilder(driver);
        var data = new Dictionary<string, object>
        {
            ["ContentTypes"] = new[] { "BlogPost" },
            ["Count"] = 3,
        };

        var context = await builder.BuildColumnsAsync<Item>("Items", data, TestContext.Current.CancellationToken);

        // The driver added a column for the content type the page is filtered by.
        Assert.Equal(["BlogPost"], context.Columns.Select(column => column.Name));

        // A key that is not there, or that holds another type, falls back instead of throwing.
        Assert.Equal(3, driver.Count);
        Assert.Null(driver.MissingKey);
        Assert.Null(driver.WrongType);
    }

    [Fact]
    public async Task BuildColumnsAsync_NoDataFromThePage_GivesDriversEmptyData()
    {
        var driver = new DataColumnDriver();
        var builder = CreateBuilder(driver);

        var context = await builder.BuildColumnsAsync<Item>("Items", cancellationToken: TestContext.Current.CancellationToken);

        Assert.Empty(context.Columns);
        Assert.Empty(driver.Data);
    }

    [Fact]
    public async Task BuildColumnsAsync_DriverAddingAColumnTheListHas_IsLoggedAndTheOtherColumnsAreKept()
    {
        var logger = new ListLogger<DefaultAdminListColumnsBuilder>();
        var builder = CreateBuilder(logger, new OwnerColumnDriver(), new PositionedColumnDriver("title", "30"), new PositionedColumnDriver("Culture", "25"));

        var context = await builder.BuildColumnsAsync<Item>("Items", cancellationToken: TestContext.Current.CancellationToken);

        // The names are compared ignoring case, so "title" is the Title column of the owner, which stays as it was.
        Assert.Equal(["Select", "Title", "Culture", "Actions"], context.Columns.Select(c => c.Name));
        Assert.Equal("20", context.Columns.Single(c => c.Name == "Title").Position);

        var error = Assert.Single(logger.Entries, entry => entry.Level == LogLevel.Error);
        Assert.IsType<ArgumentException>(error.Exception);
        Assert.Contains("already has a 'title' column", error.Exception.Message);
    }

    [Fact]
    public async Task BuildColumnsAsync_DriverRenderingAHeader_PlacesItInTheZoneOfItsColumn()
    {
        var builder = CreateBuilder(new OwnerColumnDriver(), new ScheduledColumnDriver());

        var context = await builder.BuildColumnsAsync<Item>("Items", cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(["Select", "Title", "Scheduled", "Actions"], context.Columns.Select(c => c.Name));

        // The other columns show their title.
        Assert.True(context.Headers.Zones.IsNotEmpty("Scheduled"));
        Assert.False(context.Headers.Zones.IsNotEmpty("Title"));

        var header = Assert.IsAssignableFrom<IShape>(Assert.Single(((Shape)context.Headers.Zones["Scheduled"]).Items));
        Assert.Equal("ScheduledHeader", header.Metadata.Type);
        Assert.Equal(AdminListConstants.HeaderDisplayType, header.Metadata.DisplayType);
    }

    [Fact]
    public async Task BuildCellsAsync_DriversBuildingCells_FillEachRowInOrder()
    {
        var builder = CreateBuilder(new OwnerColumnDriver(), new ScheduledColumnDriver(), new CultureColumnDriver());
        var columns = await builder.BuildColumnsAsync<Item>("Items", cancellationToken: TestContext.Current.CancellationToken);
        var first = new Shape();
        var second = new Shape();

        var cells = await builder.BuildCellsAsync(
            columns,
            [new AdminListRow<Item>(new Item("Draft", Scheduled: true), first), new AdminListRow<Item>(new Item("Published", Scheduled: false), second)],
            TestContext.Current.CancellationToken);

        Assert.Equal(2, cells.Count);

        // The driver got the item and the row shape of each row, and only placed a shape for the scheduled one.
        var scheduled = Assert.IsAssignableFrom<IShape>(Assert.Single(((Shape)cells[0].Zones["Scheduled"]).Items));
        Assert.Equal("Draft", scheduled.Properties["Name"]);
        Assert.Same(first, scheduled.Properties["Row"]);
        Assert.Equal(AdminListConstants.CellDisplayType, scheduled.Metadata.DisplayType);
        Assert.False(cells[1].Zones.IsNotEmpty("Scheduled"));

        // The columns filled by the zones of the rows have no cells of their own.
        Assert.False(cells[0].Zones.IsNotEmpty("Title"));
        Assert.False(cells[0].Zones.IsNotEmpty("Culture"));
    }

    [Fact]
    public async Task BuildCellsAsync_DriverOfAnotherList_BuildsNoCell()
    {
        var builder = CreateBuilder(new ScheduledColumnDriver());
        var columns = await builder.BuildColumnsAsync<Item>("Archive", cancellationToken: TestContext.Current.CancellationToken);

        var cells = await builder.BuildCellsAsync(columns, [new AdminListRow<Item>(new Item("Draft", Scheduled: true), new Shape())], TestContext.Current.CancellationToken);

        Assert.Empty(columns.Columns);
        Assert.False(Assert.Single(cells).Zones.IsNotEmpty("Scheduled"));
    }

    [Fact]
    public async Task BuildCellsAsync_DriverFailing_IsLoggedAndTheOtherCellsAreKept()
    {
        var logger = new ListLogger<DefaultAdminListColumnsBuilder>();
        var builder = CreateBuilder(logger, new FailingCellsDriver(), new ScheduledColumnDriver());
        var columns = await builder.BuildColumnsAsync<Item>("Items", cancellationToken: TestContext.Current.CancellationToken);

        var cells = await builder.BuildCellsAsync(columns, [new AdminListRow<Item>(new Item("Draft", Scheduled: true), new Shape())], TestContext.Current.CancellationToken);

        Assert.True(Assert.Single(cells).Zones.IsNotEmpty("Scheduled"));
        Assert.IsType<InvalidOperationException>(Assert.Single(logger.Entries, entry => entry.Level == LogLevel.Error).Exception);
    }

    [Fact]
    public void AddAdminListColumnDriver_SameDriverTwice_IsRegisteredOnce()
    {
        var services = new ServiceCollection()
            .AddAdminListColumnDriver<Item, CultureColumnDriver>()
            .AddAdminListColumnDriver<Item, CultureColumnDriver>()
            .AddAdminListColumnDriver<Item, OwnerColumnDriver>();

        Assert.Single(services, descriptor => descriptor.ImplementationType == typeof(CultureColumnDriver));
        Assert.All(services, descriptor => Assert.Equal(typeof(IAdminListColumnDriver<Item>), descriptor.ServiceType));
    }

    private static DefaultAdminListColumnsBuilder CreateBuilder(params IAdminListColumnDriver<Item>[] drivers)
        => CreateBuilder(NullLogger<DefaultAdminListColumnsBuilder>.Instance, drivers);

    // The drivers given here run in the order they are given, like the drivers of features depending on each other.
    private static DefaultAdminListColumnsBuilder CreateBuilder(ILogger<DefaultAdminListColumnsBuilder> logger, params IAdminListColumnDriver<Item>[] drivers)
    {
        return CreateBuilder(
            services =>
            {
                foreach (var driver in drivers)
                {
                    services.AddSingleton(driver);
                }
            },
            logger);
    }

    private static DefaultAdminListColumnsBuilder CreateBuilder(Action<IServiceCollection> configure, ILogger<DefaultAdminListColumnsBuilder> logger = null)
    {
        var services = new ServiceCollection();
        configure(services);

        return new DefaultAdminListColumnsBuilder(
            services.BuildServiceProvider(),
            new TestShapeFactory(),
            Mock.Of<ILayoutAccessor>(),
            Mock.Of<IUpdateModelAccessor>(),
            logger ?? NullLogger<DefaultAdminListColumnsBuilder>.Instance);
    }

    public sealed record Item(string Name, bool Scheduled = false);

    public sealed record OtherItem(string Name);

    private sealed class CountingColumnDriver : AdminListColumnDriver<Item>
    {
        public static int Created { get; set; }

        public CountingColumnDriver()
        {
            Created++;
        }

        public override IDisplayResult Columns(AdminListColumnsContext context)
        {
            context.Columns.Add(new AdminListColumn { Name = "Counted", Zones = ["Counted"] });

            return null;
        }
    }

    // Declares the columns of its list, like the module owning it does.
    private sealed class OwnerColumnDriver : AdminListColumnDriver<Item>
    {
        public override bool CanHandleList(string listName)
            => listName == "Items";

        public override IDisplayResult Columns(AdminListColumnsContext context)
        {
            context.Columns.Add(AdminListColumns.Select());
            context.Columns.Add(new AdminListColumn { Name = "Title", Position = "20", Zones = ["Title"] });
            context.Columns.Add(AdminListColumns.Actions(new LocalizedString("Actions", "Actions")));

            return null;
        }
    }

    // A column of every list of items, filled by a zone of the rows.
    private sealed class CultureColumnDriver : AdminListColumnDriver<Item>
    {
        public override IDisplayResult Columns(AdminListColumnsContext context)
        {
            context.Columns.Add(new AdminListColumn
            {
                Name = "Culture",
                Position = "25",
                Title = new LocalizedString("Culture", "Culture"),
                Zones = ["Culture"],
            });

            return null;
        }
    }

    // A column filled by a shape of its own, with a header of its own.
    private sealed class ScheduledColumnDriver : AdminListColumnDriver<Item>
    {
        public override bool CanHandleList(string listName)
            => listName == "Items";

        public override IDisplayResult Columns(AdminListColumnsContext context)
        {
            context.Columns.Add(new AdminListColumn
            {
                Name = "Scheduled",
                Position = "30",
                Title = new LocalizedString("Scheduled", "Scheduled"),
            });

            return Factory("ScheduledHeader", static () => new Shape()).Location("Scheduled");
        }

        public override IDisplayResult Cells(Item model, AdminListCellsContext context)
        {
            if (!model.Scheduled)
            {
                return null;
            }

            return Factory("ScheduledCell", static (_, state) =>
            {
                var shape = new Shape();
                shape.Properties["Name"] = state.Model.Name;
                shape.Properties["Row"] = state.Context.Row;

                return shape;
            }, (Model: model, Context: context)).Location("Scheduled:5");
        }
    }

    private sealed class FailingCellsDriver : AdminListColumnDriver<Item>
    {
        public override IDisplayResult Cells(Item model, AdminListCellsContext context)
            => throw new InvalidOperationException("The cell could not be built.");
    }

    private sealed class PositionedColumnDriver(string name, string position) : AdminListColumnDriver<Item>
    {
        public override IDisplayResult Columns(AdminListColumnsContext context)
        {
            context.Columns.Add(new AdminListColumn { Name = name, Position = position, Zones = [name] });

            return null;
        }
    }

    private sealed class RemovingColumnDriver(string name) : AdminListColumnDriver<Item>
    {
        public override IDisplayResult Columns(AdminListColumnsContext context)
        {
            Assert.NotNull(context.Find(name));
            Assert.True(context.Remove(name));
            Assert.Null(context.Find(name));

            return null;
        }
    }

    private sealed class DataColumnDriver : AdminListColumnDriver<Item>
    {
        public IReadOnlyDictionary<string, object> Data { get; private set; }

        public int Count { get; private set; }

        public string MissingKey { get; private set; }

        public string WrongType { get; private set; }

        public override IDisplayResult Columns(AdminListColumnsContext context)
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

            return null;
        }
    }

    private sealed class TestShapeFactory : IShapeFactory
    {
        public dynamic New => this;

        public async ValueTask<IShape> CreateAsync(
            string shapeType,
            Func<ValueTask<IShape>> shapeFactory,
            Action<ShapeCreatingContext> creating,
            Action<ShapeCreatedContext> created)
        {
            var shape = await shapeFactory();
            shape.Metadata.Type = shapeType;

            return shape;
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
