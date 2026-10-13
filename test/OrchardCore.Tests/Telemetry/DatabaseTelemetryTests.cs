using System.Diagnostics;
using System.Diagnostics.Metrics;
using Dapper;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Options;
using OrchardCore.Data;
using OrchardCore.Data.YesSql.Telemetry;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Builders;
using OrchardCore.Telemetry;
using OrchardCore.Tests.Apis.Context;

namespace OrchardCore.Tests.Telemetry;

public class DatabaseTelemetryTests
{
    [Theory]
    [InlineData("SELECT DISTINCT [Document].* FROM [dbo].[Document] INNER JOIN [ContentItemIndex] ON 1 = 1", "SELECT", "Document")]
    [InlineData("select * from \"t1_ContentItemIndex\" where \"Published\" = @p0", "SELECT", "t1_ContentItemIndex")]
    [InlineData("INSERT INTO `Document` (`Type`, `Content`) VALUES (@Type, @Content);", "INSERT", "Document")]
    [InlineData("UPDATE [Document] SET [Content] = @Content WHERE [Id] = @Id", "UPDATE", "Document")]
    [InlineData("DELETE FROM [Document] WHERE [Id] = @Id", "DELETE", "Document")]
    [InlineData("CREATE TABLE IF NOT EXISTS [Identifiers] ([dimension] TEXT)", "CREATE", "Identifiers")]
    [InlineData("create index [IDX_ContentItemIndex_DocumentId] on [ContentItemIndex] ([DocumentId])", "CREATE", "ContentItemIndex")]
    [InlineData("-- The next id\n/* A block comment */ SELECT COUNT(*) FROM (SELECT [Id] FROM [Document]) AS [Count]", "SELECT", "Document")]
    [InlineData("PRAGMA journal_mode", "PRAGMA", null)]
    [InlineData("   ", null, null)]
    public void ParseStatement_Sql_ReturnsOperationAndTable(string sql, string operation, string collection)
    {
        var statement = DatabaseTelemetry.ParseStatement(sql);

        Assert.Equal(operation, statement.Operation);
        Assert.Equal(collection, statement.Collection);
    }

    [Fact]
    public async Task TelemetryDbConnection_WithDapperAndTransaction_TracesEachCommand()
    {
        var activities = new ConcurrentQueue<Activity>();
        using var listener = ListenToOrchardCore(activities);

        var context = new DatabaseTelemetryContext("TelemetryTenant", "sqlite", IncludeQueryText: true);
        await using var connection = new TelemetryDbConnection(new SqliteConnection("Data Source=:memory:"), context);
        await connection.OpenAsync(TestContext.Current.CancellationToken);

        var scope = DatabaseTelemetryScope.Begin();

        await connection.ExecuteAsync("CREATE TABLE [Document] ([Id] INTEGER, [Content] TEXT)");

        // A transaction of the provider is required by SQLite: the wrapped transaction must reach the provider's command.
        await using (var transaction = await connection.BeginTransactionAsync(TestContext.Current.CancellationToken))
        {
            await connection.ExecuteAsync("INSERT INTO [Document] ([Id], [Content]) VALUES (@Id, @Content)", new { Id = 1, Content = "a" }, transaction);
            await transaction.CommitAsync(TestContext.Current.CancellationToken);
        }

        var count = await connection.ExecuteScalarAsync<long>("SELECT COUNT(*) FROM [Document]");
        var contents = (await connection.QueryAsync<string>("SELECT [Content] FROM [Document] WHERE [Id] = @Id", new { Id = 1 })).ToArray();

        Assert.Equal(1, count);
        Assert.Equal(["a"], contents);
        Assert.IsType<SqliteConnection>(connection.Unwrap());
        Assert.Equal(4, scope.OperationCount);

        var spans = activities.Where(activity => activity.GetTagItem(OrchardCoreTelemetryConstants.Attributes.TenantName) as string == "TelemetryTenant").ToArray();
        Assert.Equal(["CREATE Document", "INSERT Document", "SELECT Document", "SELECT Document"], spans.Select(span => span.DisplayName));
        Assert.All(spans, span =>
        {
            Assert.Equal(ActivityKind.Client, span.Kind);
            Assert.Equal("sqlite", span.GetTagItem(OrchardCoreTelemetryConstants.Attributes.DbSystemName));
            Assert.Equal("Document", span.GetTagItem(OrchardCoreTelemetryConstants.Attributes.DbCollectionName));
            Assert.NotNull(span.GetTagItem(OrchardCoreTelemetryConstants.Attributes.DbQueryText));
        });
    }

    [Fact]
    public async Task TelemetryDbConnection_FailingCommand_MarksSpanAsError()
    {
        var activities = new ConcurrentQueue<Activity>();
        using var listener = ListenToOrchardCore(activities);

        var context = new DatabaseTelemetryContext("FailingTenant", "sqlite", IncludeQueryText: false);
        await using var connection = new TelemetryDbConnection(new SqliteConnection("Data Source=:memory:"), context);
        await connection.OpenAsync(TestContext.Current.CancellationToken);

        await Assert.ThrowsAsync<SqliteException>(() => connection.ExecuteAsync("SELECT * FROM [Missing]"));

        var span = Assert.Single(activities, activity => activity.GetTagItem(OrchardCoreTelemetryConstants.Attributes.TenantName) as string == "FailingTenant");
        Assert.Equal(ActivityStatusCode.Error, span.Status);
        Assert.Equal(typeof(SqliteException).FullName, span.GetTagItem(OrchardCoreTelemetryConstants.Attributes.ErrorType));
        Assert.Null(span.GetTagItem(OrchardCoreTelemetryConstants.Attributes.DbQueryText));
    }

    [Fact]
    public async Task InitializeTenant_WithListeners_TracesAndMeasuresDatabaseOperations()
    {
        var activities = new ConcurrentQueue<Activity>();
        using var activityListener = ListenToOrchardCore(activities);

        var measurements = new ConcurrentQueue<(string Tenant, string System)>();
        using var meterListener = new MeterListener
        {
            InstrumentPublished = (instrument, listener) =>
            {
                if (instrument.Meter.Name == OrchardCoreTelemetryConstants.MeterName &&
                    instrument.Name == OrchardCoreTelemetryConstants.Instruments.DbClientOperationDuration)
                {
                    listener.EnableMeasurementEvents(instrument);
                }
            },
        };
        meterListener.SetMeasurementEventCallback<double>((_, _, tags, _) =>
        {
            string tenant = null, system = null;
            foreach (var tag in tags)
            {
                if (tag.Key == OrchardCoreTelemetryConstants.Attributes.TenantName)
                {
                    tenant = tag.Value as string;
                }
                else if (tag.Key == OrchardCoreTelemetryConstants.Attributes.DbSystemName)
                {
                    system = tag.Value as string;
                }
            }

            measurements.Enqueue((tenant, system));
        });
        meterListener.Start();

        using var context = new SiteContext();
        await context.InitializeAsync();

        var spans = activities
            .Where(activity => activity.Kind == ActivityKind.Client &&
                activity.GetTagItem(OrchardCoreTelemetryConstants.Attributes.TenantName) as string == context.TenantName)
            .ToArray();

        Assert.NotEmpty(spans);
        Assert.All(spans, span => Assert.Equal("sqlite", span.GetTagItem(OrchardCoreTelemetryConstants.Attributes.DbSystemName)));
        Assert.Contains(spans, span => span.GetTagItem(OrchardCoreTelemetryConstants.Attributes.DbOperationName) as string == "SELECT");
        Assert.Contains(spans, span => span.GetTagItem(OrchardCoreTelemetryConstants.Attributes.DbOperationName) as string == "INSERT");
        Assert.Contains((context.TenantName, "sqlite"), measurements);
    }

    [Fact]
    public async Task TenantTelemetryMiddleware_DatabaseOperations_TagsRequestSpan()
    {
        using var activity = new Activity("Request").Start();

        var httpContext = new DefaultHttpContext();
        httpContext.Features.Set<IHttpActivityFeature>(new ActivityFeature { Activity = activity });

        var middleware = new TenantTelemetryMiddleware(
            context =>
            {
                context.Features.Set(new ShellContextFeature
                {
                    ShellContext = new ShellContext { Settings = new ShellSettings { Name = "Tenant1" } },
                });

                DatabaseTelemetryScope.Current.RecordOperation(TimeSpan.FromMilliseconds(20));
                DatabaseTelemetryScope.Current.RecordOperation(TimeSpan.FromMilliseconds(30));

                return Task.CompletedTask;
            },
            Options.Create(new OrchardCoreOpenTelemetryOptions()));

        await middleware.InvokeAsync(httpContext);

        Assert.Equal(2, activity.GetTagItem(OrchardCoreTelemetryConstants.Attributes.DbOperations));
        Assert.Equal(0.05, (double)activity.GetTagItem(OrchardCoreTelemetryConstants.Attributes.DbDuration), precision: 6);
    }

    private static ActivityListener ListenToOrchardCore(ConcurrentQueue<Activity> activities)
    {
        var listener = new ActivityListener
        {
            ShouldListenTo = source => source.Name == OrchardCoreTelemetryConstants.ActivitySourceName,
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllDataAndRecorded,
            ActivityStopped = activities.Enqueue,
        };
        ActivitySource.AddActivityListener(listener);

        return listener;
    }

    private sealed class ActivityFeature : IHttpActivityFeature
    {
        public Activity Activity { get; set; }
    }
}
