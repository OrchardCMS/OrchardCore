using System.Runtime.CompilerServices;
using OrchardCore.DataSources;
using OrchardCore.DataSources.Operations;
using OrchardCore.Localization;

namespace OrchardCore.Tests.Modules.OrchardCore.DataSources;

public sealed class RecordDataSourceTests
{
    private static readonly Ticket[] _tickets =
    [
        new("T1", TicketState.Open, new DateTimeOffset(2026, 3, 1, 9, 0, 0, TimeSpan.FromHours(-5)), 3),
        new("T2", TicketState.Closed, new DateTimeOffset(2026, 3, 2, 10, 0, 0, TimeSpan.Zero), 1),
        new("T3", TicketState.Open, new DateTimeOffset(2026, 3, 5, 11, 0, 0, TimeSpan.Zero), 2),
    ];

    [Fact]
    public async Task ReadAsync_RecordDataSet_ConvertsTheValuesOfEveryField()
    {
        // Arrange
        var source = new TicketsSource();
        var query = new DataSourceQuery { DataSet = "Tickets", Context = Context(canRead: true) };

        // Act
        var batches = await DataTestHelpers.ToListAsync(source.ReadAsync(query, TestContext.Current.CancellationToken));

        // Assert
        var batch = Assert.Single(batches);
        Assert.Equal(["Id", "State", "Opened", "Priority"], batch.Fields.Select(field => field.Name));
        Assert.Equal(["T1", "Open", new DateTime(2026, 3, 1, 14, 0, 0, DateTimeKind.Utc), 3L], batch.Rows[0]);
        Assert.Equal(DateTimeKind.Utc, ((DateTime)batch.Rows[0][2]).Kind);
    }

    [Fact]
    public async Task ReadAsync_SelectedFieldsMaxRowsAndBatchSize_ReadsOnlyThose()
    {
        // Arrange
        var source = new TicketsSource();
        var query = new DataSourceQuery
        {
            DataSet = "Tickets",
            Fields = new HashSet<string>(["Priority", "Id"], StringComparer.Ordinal),
            MaxRows = 2,
            BatchSize = 1,
            Context = Context(canRead: true),
        };

        // Act
        var batches = await DataTestHelpers.ToListAsync(source.ReadAsync(query, TestContext.Current.CancellationToken));

        // Assert: the fields keep the order of the data set.
        Assert.Equal(2, batches.Count);
        Assert.All(batches, batch => Assert.Equal(["Id", "Priority"], batch.Fields.Select(field => field.Name)));
        Assert.Equal(["T1", "T2"], batches.SelectMany(batch => batch.Rows).Select(row => row[0]));
    }

    [Fact]
    public async Task ReadAsync_KeyCondition_LetsTheDataSetLoadOnlyThoseRecords()
    {
        // Arrange
        var source = new TicketsSource();
        var query = new DataSourceQuery
        {
            DataSet = "Tickets",
            Conditions = [new DataCondition { Field = "Id", Operator = DataFilterOperator.In, Values = ["T3", "T1"], IsJoinKey = true }],
            Context = Context(canRead: true),
        };

        // Act
        var batches = await DataTestHelpers.ToListAsync(source.ReadAsync(query, TestContext.Current.CancellationToken));

        // Assert
        Assert.Equal(["T1", "T3"], batches.SelectMany(batch => batch.Rows).Select(row => row[0]));
    }

    [Fact]
    public async Task UserWhoCannotRead_GetsNoDataSetSchemaOrRows()
    {
        // Arrange
        var source = new TicketsSource();
        var context = Context(canRead: false);

        // Act
        var dataSets = await source.GetDataSetsAsync(context, TestContext.Current.CancellationToken);
        var schema = await source.GetSchemaAsync("Tickets", context, TestContext.Current.CancellationToken);
        var batches = await DataTestHelpers.ToListAsync(source.ReadAsync(new DataSourceQuery { DataSet = "Tickets", Context = context }, TestContext.Current.CancellationToken));

        // Assert
        Assert.Empty(dataSets);
        Assert.Null(schema);
        Assert.Empty(batches);
    }

    [Fact]
    public async Task GetSchemaAsync_UserWhoCanRead_DescribesTheFields()
    {
        // Act
        var schema = await new TicketsSource().GetSchemaAsync("Tickets", Context(canRead: true), TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal("Tickets", schema.DataSet.Name);
        Assert.True(schema.FindField("Id").IsKeyFilterable);
        Assert.Equal(DataFieldType.DateTime, schema.FindField("Opened").Type);
    }

    [Fact]
    public void Keys_SeveralConditions_KeepsTheValuesAllOfThemAllow()
    {
        // Arrange
        DataCondition[] conditions =
        [
            new() { Field = "Id", Operator = DataFilterOperator.In, Values = ["a", "b", "c"] },
            new() { Field = "Id", Operator = DataFilterOperator.Equals, Values = ["b"] },
            new() { Field = "Other", Operator = DataFilterOperator.In, Values = ["x"] },
        ];

        // Act & Assert
        Assert.Equal(["b"], DataConditions.Keys(conditions, "Id"));
        Assert.Null(DataConditions.Keys(conditions, "Missing"));
    }

    [Fact]
    public void DateRange_SeveralConditions_ReturnsTheNarrowestRange()
    {
        // Arrange
        var march = new DateTime(2026, 3, 1, 0, 0, 0, DateTimeKind.Utc);
        DataCondition[] conditions =
        [
            new() { Field = "Opened", Operator = DataFilterOperator.Between, Values = [march, march.AddMonths(1)] },
            new() { Field = "Opened", Operator = DataFilterOperator.GreaterThan, Values = [march.AddDays(10)] },
            new() { Field = "Opened", Operator = DataFilterOperator.Contains, Values = ["x"] },
        ];

        // Act
        var (from, to) = DataConditions.DateRange(conditions, "Opened");

        // Assert
        Assert.Equal(march.AddDays(10), from);
        Assert.Equal(march.AddMonths(1), to);
        Assert.Equal((null, null), DataConditions.DateRange(conditions, "Other"));
    }

    [Theory]
    [InlineData(DataFieldTransform.Year, 2026L)]
    [InlineData(DataFieldTransform.Quarter, 2L)]
    [InlineData(DataFieldTransform.MonthOfYear, 5L)]
    [InlineData(DataFieldTransform.DayOfWeek, 7L)]
    public void Apply_DateTransform_ReturnsTheNumber(DataFieldTransform transform, long expected)
    {
        // Arrange: a Sunday.
        var date = new DateTime(2026, 5, 17, 15, 45, 30, DateTimeKind.Utc);

        // Act & Assert
        Assert.Equal(expected, DataFieldTransforms.Apply(transform, date));
        Assert.Equal(DataFieldType.Integer, DataFieldTransforms.GetResultType(transform, DataFieldType.DateTime));
    }

    [Fact]
    public void Apply_DateTransforms_TruncateTheDate()
    {
        // Arrange: a Sunday.
        var date = new DateTime(2026, 5, 17, 15, 45, 30, DateTimeKind.Utc);

        // Act & Assert
        Assert.Equal(new DateTime(2026, 5, 1, 0, 0, 0, DateTimeKind.Utc), DataFieldTransforms.Apply(DataFieldTransform.Month, date));
        Assert.Equal(new DateTime(2026, 5, 11, 0, 0, 0, DateTimeKind.Utc), DataFieldTransforms.Apply(DataFieldTransform.Week, date));
        Assert.Equal(new DateTime(2026, 5, 17, 0, 0, 0, DateTimeKind.Utc), DataFieldTransforms.Apply(DataFieldTransform.Day, date));
        Assert.Equal(new DateTime(2026, 5, 17, 15, 0, 0, DateTimeKind.Utc), DataFieldTransforms.Apply(DataFieldTransform.Hour, date));
    }

    [Fact]
    public void Apply_TextAndNumberTransforms_ConvertTheValue()
    {
        // Act & Assert
        Assert.Equal("ABC", DataFieldTransforms.Apply(DataFieldTransform.Upper, "abc"));
        Assert.Equal("a b", DataFieldTransforms.Apply(DataFieldTransform.Trim, "  a b "));
        Assert.Equal(3L, DataFieldTransforms.Apply(DataFieldTransform.Length, "abc"));
        Assert.Equal(3L, DataFieldTransforms.Apply(DataFieldTransform.Round, 2.5m));
        Assert.Null(DataFieldTransforms.Apply(DataFieldTransform.Year, "not a date"));
        Assert.False(DataFieldTransforms.Supports(DataFieldTransform.Year, DataFieldType.Text));
        Assert.True(DataFieldTransforms.Supports(DataFieldTransform.Round, DataFieldType.Decimal));
    }

    [Fact]
    public void WithName_FieldWithReferences_KeepsThem()
    {
        // Arrange
        var field = new DataField("CustomerId", "Customer", DataFieldType.Text)
        {
            IsIdentifier = true,
            IsKeyFilterable = true,
            References = [new DataFieldReference("Contents", "Customer", "ContentItemId")],
        };

        // Act
        var copy = field.WithName("Right.CustomerId");

        // Assert
        Assert.True(copy.IsIdentifier);
        Assert.False(copy.IsKeyFilterable);
        Assert.Single(copy.References);
        Assert.NotSame(field.References, copy.References);
    }

    [Fact]
    public async Task AggregateAsync_DataSetThatAggregates_ReturnsItsGroups()
    {
        // Arrange
        var query = new DataAggregateQuery
        {
            DataSet = "IndexedTickets",
            Groups = [new DataAggregateGroup { Field = "State" }],
            Measures = [new DataAggregateMeasure { Function = DataAggregate.Count }],
            Context = Context(canRead: true),
        };

        // Act
        var groups = await new TicketsSource().AggregateAsync(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal([["Open", 2L], ["Closed", 1L]], groups);
    }

    [Fact]
    public async Task AggregateAsync_DataSetThatDoesNotAggregate_ReturnsNull()
    {
        // Arrange
        var query = new DataAggregateQuery
        {
            DataSet = "Tickets",
            Measures = [new DataAggregateMeasure { Function = DataAggregate.Count }],
            Context = Context(canRead: true),
        };

        // Act & Assert
        Assert.Null(await new TicketsSource().AggregateAsync(query, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task AggregateAsync_UserWhoCannotRead_ReturnsNull()
    {
        // Arrange
        var query = new DataAggregateQuery
        {
            DataSet = "IndexedTickets",
            Measures = [new DataAggregateMeasure { Function = DataAggregate.Count }],
            Context = Context(canRead: false),
        };

        // Act & Assert
        Assert.Null(await new TicketsSource().AggregateAsync(query, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task GetDataSetsAsync_FieldsWithReferences_ListTheDataSetsTheyReference()
    {
        // Act
        var dataSets = await new TicketsSource().GetDataSetsAsync(Context(canRead: true), TestContext.Current.CancellationToken);
        var schema = await new TicketsSource().GetSchemaAsync("Assignments", Context(canRead: true), TestContext.Current.CancellationToken);

        // Assert: the two fields that reference tickets list the tickets once.
        var assignments = Assert.Single(dataSets, dataSet => dataSet.Name == "Assignments");
        var reference = Assert.Single(assignments.References);
        Assert.Equal(("Tickets", "Tickets", "Id"), (reference.Source, reference.DataSet, reference.Field));
        Assert.Single(schema.DataSet.References);
        Assert.Empty(Assert.Single(dataSets, dataSet => dataSet.Name == "Tickets").References);
    }

    private static DataSourceContext Context(bool canRead)
    {
        var context = new DataSourceContext();
        context.Properties["CanRead"] = canRead;

        return context;
    }

    private enum TicketState
    {
        Open,
        Closed,
    }

    private sealed record Ticket(string Id, TicketState State, DateTimeOffset Opened, int Priority);

    private sealed class TicketsSource : RecordDataSource, IDataSource
    {
        public static string Name => "Tickets";

        public static LocalizationSource DisplayName { get; } = new("Tickets");

        public static LocalizationSource Description { get; } = new("Tickets");

        protected override IEnumerable<IRecordDataSet> DataSets { get; } = [new TicketsDataSet(), new IndexedTicketsDataSet(), new AssignmentsDataSet()];
    }

    private class TicketsDataSet : RecordDataSet<Ticket>
    {
        public TicketsDataSet()
            : this("Tickets")
        {
        }

        protected TicketsDataSet(string name)
            : base(new DataSetDescriptor(name, name))
        {
            AddField("Id", "Ticket", DataFieldType.Text, ticket => ticket.Id).IsKeyFilterable = true;
            AddField("State", "State", DataFieldType.Text, ticket => ticket.State);
            AddField("Opened", "Opened", DataFieldType.DateTime, ticket => ticket.Opened);
            AddField("Priority", "Priority", DataFieldType.Integer, ticket => ticket.Priority);
        }

        public override Task<bool> CanReadAsync(DataSourceContext context, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(context.Properties.TryGetValue("CanRead", out var canRead) && canRead is true);
        }

        protected override async IAsyncEnumerable<Ticket> LoadAsync(DataSourceQuery query, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var keys = DataConditions.Keys(query.Conditions, "Id");

            foreach (var ticket in _tickets.Where(ticket => keys is null || keys.Contains(ticket.Id)))
            {
                await Task.Yield();

                yield return ticket;
            }
        }
    }

    // Counts the tickets of each state itself, the way a data set over an index would with a GROUP BY.
    private sealed class IndexedTicketsDataSet : TicketsDataSet, IAggregateRecordDataSet
    {
        public IndexedTicketsDataSet()
            : base("IndexedTickets")
        {
        }

        public Task<IReadOnlyList<object[]>> AggregateAsync(DataAggregateQuery query, CancellationToken cancellationToken = default)
        {
            if (query.Groups is not [{ Field: "State", Boundaries: null }] || query.Measures is not [{ Function: DataAggregate.Count, Field: null }])
            {
                return Task.FromResult<IReadOnlyList<object[]>>(null);
            }

            return Task.FromResult<IReadOnlyList<object[]>>(_tickets
                .GroupBy(ticket => ticket.State)
                .Select(group => new object[] { group.Key.ToString(), (long)group.Count() })
                .ToList());
        }
    }

    private sealed record Assignment(string TicketId, string PreviousTicketId, string Agent);

    private sealed class AssignmentsDataSet : RecordDataSet<Assignment>
    {
        public AssignmentsDataSet()
            : base(new DataSetDescriptor("Assignments", "Assignments"))
        {
            AddField("TicketId", "Ticket", DataFieldType.Text, assignment => assignment.TicketId).References.Add(new DataFieldReference("Tickets", "Tickets", "Id"));
            AddField("PreviousTicketId", "Previous ticket", DataFieldType.Text, assignment => assignment.PreviousTicketId).References.Add(new DataFieldReference("Tickets", "Tickets", "Id"));
            AddField("Agent", "Agent", DataFieldType.Text, assignment => assignment.Agent);
        }

        public override Task<bool> CanReadAsync(DataSourceContext context, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(context.Properties.TryGetValue("CanRead", out var canRead) && canRead is true);
        }

        protected override async IAsyncEnumerable<Assignment> LoadAsync(DataSourceQuery query, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            await Task.Yield();

            yield return new Assignment("T1", null, "Ada");
        }
    }
}
