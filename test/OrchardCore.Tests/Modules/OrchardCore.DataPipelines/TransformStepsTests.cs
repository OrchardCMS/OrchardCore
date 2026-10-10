using OrchardCore.DataPipelines.Steps;
using OrchardCore.DataSources;
using OrchardCore.Tests.Modules.OrchardCore.DataSources;

namespace OrchardCore.Tests.Modules.OrchardCore.DataPipelines;

public sealed class TransformStepsTests
{
    private static readonly DataField[] _orders =
    [
        new("Id", "Id", DataFieldType.Integer),
        new("Customer", "Customer", DataFieldType.Text),
        new("Amount", "Amount", DataFieldType.Decimal),
        new("Quantity", "Quantity", DataFieldType.Integer),
        new("Created", "Created", DataFieldType.DateTime),
    ];

    private static readonly object[][] _orderRows =
    [
        [1L, "Ada", 10.5m, 2L, new DateTime(2026, 1, 5, 10, 0, 0, DateTimeKind.Utc)],
        [2L, "Bob", 99m, 1L, new DateTime(2026, 2, 1, 9, 0, 0, DateTimeKind.Utc)],
        [3L, "Ada", 5m, 4L, new DateTime(2026, 2, 3, 8, 0, 0, DateTimeKind.Utc)],
        [4L, "Cy", null, 3L, null],
        [5L, "bob", 20m, 2L, new DateTime(2026, 3, 1, 7, 0, 0, DateTimeKind.Utc)],
    ];

    [Fact]
    public async Task Filter_Condition_SplitsMatchedAndUnmatchedRows()
    {
        // Arrange
        using var host = new StepTestHost(new FilterStep(Localizer<FilterStep>()), new FilterStepSettings { Condition = "[Amount] >= 10" })
            .WithRows(_orders, _orderRows);

        // Act
        await host.ExecuteAsync(TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal([1L, 2L, 5L], host.Output(FilterStep.Matched).Rows.Select(row => row[0]));
        Assert.Equal([3L, 4L], host.Output(FilterStep.Unmatched).Rows.Select(row => row[0]));
    }

    [Theory]
    [InlineData("", "Enter")]
    [InlineData("[Missing] > 1", "Missing")]
    [InlineData("[Amount] + 1", "true or false")]
    [InlineData("SUM([Amount]) > 1", "aggregate")]
    public async Task Filter_InvalidCondition_ReportsError(string condition, string expected)
    {
        // Arrange
        using var host = new StepTestHost(new FilterStep(Localizer<FilterStep>()), new FilterStepSettings { Condition = condition })
            .WithRows(_orders);

        // Act
        var context = await host.DescribeAsync();

        // Assert
        var issue = Assert.Single(context.Issues);
        Assert.Equal(DataPipelineIssueSeverity.Error, issue.Severity);
        Assert.Contains(expected, issue.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CalculatedFields_Formulas_AddTypedFieldsThatCanReferEachOther()
    {
        // Arrange
        var settings = new CalculatedFieldsStepSettings
        {
            Fields =
            [
                new CalculatedField { Name = "Total", Formula = "[Amount] * [Quantity]" },
                new CalculatedField { Name = "Label", Formula = "UPPER([Customer]) & ': ' & [Total]" },
                new CalculatedField { Name = "Customer", Formula = "PROPER([Customer])" },
            ],
        };
        using var host = new StepTestHost(new CalculatedFieldsStep(Localizer<CalculatedFieldsStep>()), settings).WithRows(_orders, _orderRows);

        // Act
        var description = await host.DescribeAsync();
        await host.ExecuteAsync(TestContext.Current.CancellationToken);

        // Assert
        var fields = description.Outputs[DataPipelinePort.Output];
        Assert.Equal(["Id", "Customer", "Amount", "Quantity", "Created", "Total", "Label"], fields.Select(field => field.Name));
        Assert.Equal(DataFieldType.Decimal, fields.Find("Total").Type);
        Assert.Equal(DataFieldType.Text, fields.Find("Label").Type);

        var output = host.Output();
        Assert.Equal(21m, output.Value(0, "Total"));
        Assert.Equal("ADA: 21", output.Value(0, "Label"));
        Assert.Equal("Bob", output.Value(4, "Customer"));
        Assert.Null(output.Value(3, "Total"));
    }

    [Fact]
    public async Task CalculatedFields_EmptyName_ReportsError()
    {
        // Arrange
        var settings = new CalculatedFieldsStepSettings { Fields = [new CalculatedField { Name = " ", Formula = "1" }] };
        using var host = new StepTestHost(new CalculatedFieldsStep(Localizer<CalculatedFieldsStep>()), settings).WithRows(_orders);

        // Act
        var context = await host.DescribeAsync();

        // Assert
        Assert.Contains(context.Issues, issue => issue.Severity == DataPipelineIssueSeverity.Error);
    }

    [Fact]
    public async Task SelectFields_RenamesReordersAndConvertsTypes()
    {
        // Arrange
        var settings = new SelectFieldsStepSettings
        {
            Fields =
            [
                new SelectedField { Field = "Customer", Name = "Name" },
                new SelectedField { Field = "Amount", Type = DataFieldType.Integer },
                new SelectedField { Field = "Id", Type = DataFieldType.Text },
            ],
        };
        using var host = new StepTestHost(new SelectFieldsStep(Localizer<SelectFieldsStep>()), settings).WithRows(_orders, _orderRows);

        // Act
        var description = await host.DescribeAsync();
        await host.ExecuteAsync(TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(["Name", "Amount", "Id"], description.Outputs[DataPipelinePort.Output].Select(field => field.Name));
        Assert.Equal(["Ada", 10L, "1"], host.Output().Rows[0]);
    }

    [Fact]
    public async Task SelectFields_UnknownField_ReportsError()
    {
        // Arrange
        var settings = new SelectFieldsStepSettings { Fields = [new SelectedField { Field = "Nope" }] };
        using var host = new StepTestHost(new SelectFieldsStep(Localizer<SelectFieldsStep>()), settings).WithRows(_orders);

        // Act
        var context = await host.DescribeAsync();

        // Assert
        Assert.Contains(context.Issues, issue => issue.Message.Contains("Nope", StringComparison.Ordinal));
    }

    [Fact]
    public async Task Sort_SeveralKeys_SortsAcrossBatches()
    {
        // Arrange
        var settings = new SortStepSettings
        {
            Keys = [new SortField { Field = "Customer" }, new SortField { Field = "Amount", Descending = true }],
        };
        using var host = new StepTestHost(new SortStep(Localizer<SortStep>()), settings).WithRows(_orders, _orderRows);

        // Act
        await host.ExecuteAsync(TestContext.Current.CancellationToken);

        // Assert: text sorts ignore case, and rows with equal keys keep their order.
        Assert.Equal([1L, 3L, 2L, 5L, 4L], host.Output().Rows.Select(row => row[0]));
    }

    [Fact]
    public async Task Aggregate_GroupsAndComputesMeasures()
    {
        // Arrange
        var settings = new AggregateStepSettings
        {
            GroupBy = ["Customer"],
            Measures =
            [
                new AggregateMeasure { Name = "Orders", Function = DataAggregate.Count },
                new AggregateMeasure { Name = "Revenue", Function = DataAggregate.Sum, Field = "Amount" },
                new AggregateMeasure { Name = "Average", Function = DataAggregate.Average, Field = "Amount" },
                new AggregateMeasure { Name = "Largest", Function = DataAggregate.Max, Field = "Quantity" },
                new AggregateMeasure { Name = "Last", Function = DataAggregate.Max, Field = "Created" },
            ],
        };
        using var host = new StepTestHost(new AggregateStep(Localizer<AggregateStep>()), settings).WithRows(_orders, _orderRows);

        // Act
        var description = await host.DescribeAsync();
        await host.ExecuteAsync(TestContext.Current.CancellationToken);

        // Assert
        var fields = description.Outputs[DataPipelinePort.Output];
        Assert.Equal(["Customer", "Orders", "Revenue", "Average", "Largest", "Last"], fields.Select(field => field.Name));
        Assert.Equal(DataFieldType.Integer, fields.Find("Orders").Type);
        Assert.Equal(DataFieldType.Decimal, fields.Find("Revenue").Type);
        Assert.Equal(DataFieldType.DateTime, fields.Find("Last").Type);

        var rows = host.Output().Rows;
        Assert.Equal(4, rows.Count);
        var ada = rows.Single(row => (string)row[0] == "Ada");
        Assert.Equal([2L, 15.5m, 7.75m, 4L, new DateTime(2026, 2, 3, 8, 0, 0)], ada.Skip(1));
        var cy = rows.Single(row => (string)row[0] == "Cy");
        Assert.Equal(1L, cy[1]);
        Assert.Null(cy[2]);
    }

    [Fact]
    public async Task Aggregate_NoGroups_ProducesOneTotalRow()
    {
        // Arrange
        var settings = new AggregateStepSettings
        {
            Measures =
            [
                new AggregateMeasure { Name = "Customers", Function = DataAggregate.CountDistinct, Field = "Customer" },
                new AggregateMeasure { Name = "Median", Function = DataAggregate.Median, Field = "Quantity" },
            ],
        };
        using var host = new StepTestHost(new AggregateStep(Localizer<AggregateStep>()), settings).WithRows(_orders, _orderRows);

        // Act
        await host.ExecuteAsync(TestContext.Current.CancellationToken);

        // Assert: values compare exactly, so "Bob" and "bob" are two customers.
        var row = Assert.Single(host.Output().Rows);
        Assert.Equal(4L, row[0]);
        Assert.Equal(2m, Convert.ToDecimal(row[1], CultureInfo.InvariantCulture));
    }

    [Theory]
    [InlineData(DataJoinType.Inner, new[] { "1:Gold", "2:Silver", "3:Gold" })]
    [InlineData(DataJoinType.Left, new[] { "1:Gold", "2:Silver", "3:Gold", "4:", "5:" })]
    [InlineData(DataJoinType.Right, new[] { "1:Gold", "2:Silver", "3:Gold", ":Bronze" })]
    [InlineData(DataJoinType.Full, new[] { "1:Gold", "2:Silver", "3:Gold", "4:", "5:", ":Bronze" })]
    public async Task Join_JoinTypes_MatchRowsOnKeys(DataJoinType joinType, string[] expected)
    {
        // Arrange
        DataField[] customers = [new("Name", "Name", DataFieldType.Text), new("Tier", "Tier", DataFieldType.Text)];
        var settings = new JoinStepSettings
        {
            JoinType = joinType,
            Keys = [new JoinKey { LeftField = "Customer", RightField = "Name" }],
        };
        using var host = new StepTestHost(new JoinStep(Localizer<JoinStep>()), settings)
            .WithInput(JoinStep.Left, _orders, _orderRows)
            .WithInput(JoinStep.Right, customers, ["Ada", "Gold"], ["Bob", "Silver"], ["Dee", "Bronze"]);

        // Act
        var description = await host.DescribeAsync();
        await host.ExecuteAsync(TestContext.Current.CancellationToken);

        // Assert
        var fields = description.Outputs[DataPipelinePort.Output];
        Assert.Equal(["Id", "Customer", "Amount", "Quantity", "Created", "Name", "Tier"], fields.Select(field => field.Name));

        var output = host.Output();
        var actual = output.Rows.Select(row => $"{row[0]}:{row[6]}").ToArray();
        Assert.Equal(expected, actual);
    }

    [Fact]
    public async Task Join_IgnoreCase_MatchesTextKeysIgnoringCase()
    {
        // Arrange
        DataField[] customers = [new("Name", "Name", DataFieldType.Text), new("Tier", "Tier", DataFieldType.Text)];
        var settings = new JoinStepSettings
        {
            JoinType = DataJoinType.Inner,
            IgnoreCase = true,
            Keys = [new JoinKey { LeftField = "Customer", RightField = "Name" }],
        };
        using var host = new StepTestHost(new JoinStep(Localizer<JoinStep>()), settings)
            .WithInput(JoinStep.Left, _orders, _orderRows)
            .WithInput(JoinStep.Right, customers, ["BOB", "Silver"]);

        // Act
        await host.ExecuteAsync(TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal([2L, 5L], host.Output().Rows.Select(row => row[0]));
    }

    [Fact]
    public async Task Join_ConflictingFieldNames_PrefixesRightFields()
    {
        // Arrange
        DataField[] other = [new("Id", "Id", DataFieldType.Integer), new("Customer", "Customer", DataFieldType.Text)];
        var settings = new JoinStepSettings { Keys = [new JoinKey { LeftField = "Id", RightField = "Id" }] };
        using var host = new StepTestHost(new JoinStep(Localizer<JoinStep>()), settings)
            .WithInput(JoinStep.Left, _orders)
            .WithInput(JoinStep.Right, other);

        // Act
        var description = await host.DescribeAsync();

        // Assert
        Assert.Equal(
            ["Id", "Customer", "Amount", "Quantity", "Created", "Right.Id", "Right.Customer"],
            description.Outputs[DataPipelinePort.Output].Select(field => field.Name));
    }

    [Fact]
    public async Task Union_DifferentFields_AlignsColumnsByName()
    {
        // Arrange
        DataField[] first = [new("Id", "Id", DataFieldType.Integer), new("Name", "Name", DataFieldType.Text)];
        DataField[] second = [new("Name", "Name", DataFieldType.Text), new("Email", "Email", DataFieldType.Text), new("Id", "Id", DataFieldType.Decimal)];
        using var host = new StepTestHost(new UnionStep(Localizer<UnionStep>()))
            .WithRows(first, [1L, "a"])
            .WithRows(second, ["b", "b@x", 2.5m]);

        // Act
        var description = await host.DescribeAsync();
        await host.ExecuteAsync(TestContext.Current.CancellationToken);

        // Assert
        var fields = description.Outputs[DataPipelinePort.Output];
        Assert.Equal(["Id", "Name", "Email"], fields.Select(field => field.Name));
        Assert.Equal(DataFieldType.Decimal, fields.Find("Id").Type);
        Assert.Equal([1m, "a", null], host.Output().Rows[0]);
        Assert.Equal([2.5m, "b", "b@x"], host.Output().Rows[1]);
    }

    [Fact]
    public async Task Distinct_KeyFields_KeepsFirstRowOfEachKey()
    {
        // Arrange
        var settings = new DistinctStepSettings { Fields = ["Customer"] };
        using var host = new StepTestHost(new DistinctStep(Localizer<DistinctStep>()), settings).WithRows(_orders, _orderRows);

        // Act
        await host.ExecuteAsync(TestContext.Current.CancellationToken);

        // Assert: keys compare exactly, so "Bob" and "bob" are different.
        Assert.Equal([1L, 2L, 4L, 5L], host.Output().Rows.Select(row => row[0]));
    }

    [Fact]
    public async Task Limit_SkipAndCount_KeepsRequestedRows()
    {
        // Arrange
        var settings = new LimitStepSettings { Skip = 1, Count = 3 };
        using var host = new StepTestHost(new LimitStep(Localizer<LimitStep>()), settings).WithRows(_orders, _orderRows);

        // Act
        await host.ExecuteAsync(TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal([2L, 3L, 4L], host.Output().Rows.Select(row => row[0]));
    }

    private static PassThroughStringLocalizer<T> Localizer<T>() => new();
}
