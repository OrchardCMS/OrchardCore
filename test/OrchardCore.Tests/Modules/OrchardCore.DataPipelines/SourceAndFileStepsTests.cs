using OrchardCore.DataPipelines.Steps;
using OrchardCore.DataSources;
using OrchardCore.DataSources.Files;
using OrchardCore.Tests.Modules.OrchardCore.DataSources;

namespace OrchardCore.Tests.Modules.OrchardCore.DataPipelines;

public sealed class SourceAndFileStepsTests
{
    private static readonly DataField[] _people =
    [
        new("Id", "Id", DataFieldType.Integer),
        new("Name", "Name", DataFieldType.Text),
        new("Joined", "Joined", DataFieldType.DateTime),
    ];

    private static readonly object[][] _peopleRows =
    [
        [1L, "Ada", new DateTime(2026, 1, 10, 0, 0, 0, DateTimeKind.Utc)],
        [2L, "Bob", new DateTime(2026, 2, 10, 0, 0, 0, DateTimeKind.Utc)],
        [3L, "Cy", new DateTime(2026, 3, 10, 0, 0, 0, DateTimeKind.Utc)],
    ];

    [Fact]
    public async Task DataSource_SelectedFieldsAndFilter_ReadsMatchingRows()
    {
        // Arrange
        var source = new InMemoryDataSource().Add("People", _people, _peopleRows);
        var settings = new DataSourceStepSettings
        {
            Source = "Memory",
            DataSet = "People",
            Fields = ["Name", "Id"],
            Filters = [new DataSourceFilter { Field = "Joined", Operator = DataFilterOperator.GreaterThanOrEqual, Values = ["2026-02-01"] }],
        };
        using var host = new StepTestHost(new DataSourceStep(new PassThroughStringLocalizer<DataSourceStep>()), settings, Services(source));

        // Act
        var description = await host.DescribeAsync();
        await host.ExecuteAsync(TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(["Name", "Id"], description.Outputs[DataPipelinePort.Output].Select(field => field.Name));
        Assert.Equal([["Bob", 2L], ["Cy", 3L]], host.Output().Rows);

        var query = Assert.Single(source.Queries);
        Assert.Same(host.Run.User, query.Context.User);
        Assert.Contains(query.Conditions, condition => condition.Field == "Joined");
    }

    [Fact]
    public async Task DataSource_SourceAsksToConfirm_RunsTheActionWhenTheRunSucceeds()
    {
        // Arrange
        var source = new InMemoryDataSource().Add("People", _people, _peopleRows);
        source.OnRead = query => query.Context.OnSucceeded((_, _) => Task.CompletedTask);
        var settings = new DataSourceStepSettings { Source = "Memory", DataSet = "People" };
        using var host = new StepTestHost(new DataSourceStep(new PassThroughStringLocalizer<DataSourceStep>()), settings, Services(source));

        // Act
        await host.ExecuteAsync(TestContext.Current.CancellationToken);

        // Assert
        Assert.Single(host.Run.SucceededActions);
        Assert.Equal($"{host.Run.PipelineId}/step", Assert.Single(source.Queries).Context.ConsumerId);
    }

    [Fact]
    public async Task DataSource_PreviewOfASourceThatAsksToConfirm_ConfirmsNothing()
    {
        // Arrange
        var source = new InMemoryDataSource().Add("People", _people, _peopleRows);
        source.OnRead = query => query.Context.OnSucceeded((_, _) => Task.CompletedTask);
        var settings = new DataSourceStepSettings { Source = "Memory", DataSet = "People" };
        using var host = new StepTestHost(new DataSourceStep(new PassThroughStringLocalizer<DataSourceStep>()), settings, Services(source));
        host.Run.IsPreview = true;

        // Act
        await host.ExecuteAsync(TestContext.Current.CancellationToken);

        // Assert
        Assert.Empty(host.Run.SucceededActions);
        Assert.Null(Assert.Single(source.Queries).Context.ConsumerId);
    }

    [Fact]
    public async Task DataSource_IncrementalFirstRun_ReadsEveryRowAndKeepsTheLargestValue()
    {
        // Arrange
        var source = new InMemoryDataSource().Add("People", _people, _peopleRows);
        var settings = new DataSourceStepSettings { Source = "Memory", DataSet = "People", IncrementalField = "Joined" };
        using var host = new StepTestHost(new DataSourceStep(new PassThroughStringLocalizer<DataSourceStep>()), settings, Services(source));

        // Act
        await host.ExecuteAsync(TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(3, host.Output().Rows.Count);
        Assert.True(host.Run.State.IsChanged);
        Assert.Equal("2026-03-10T00:00:00.0000000Z", host.Run.State.Get<string>(DataPipelineStepContext.GetStateKey("step", DataSourceStep.WatermarkStateKey)));
    }

    [Fact]
    public async Task DataSource_IncrementalNextRun_ReadsOnlyTheRowsAfterTheSavedValue()
    {
        // Arrange
        var source = new InMemoryDataSource().Add("People", _people, _peopleRows);
        var settings = new DataSourceStepSettings { Source = "Memory", DataSet = "People", IncrementalField = "Joined" };
        using var host = new StepTestHost(new DataSourceStep(new PassThroughStringLocalizer<DataSourceStep>()), settings, Services(source));
        host.Run.State.Set(DataPipelineStepContext.GetStateKey("step", DataSourceStep.WatermarkStateKey), "2026-02-10T00:00:00Z");
        host.Run.State.Set(DataPipelineStepContext.GetStateKey("step", DataSourceStep.WatermarkFieldStateKey), "Memory/People/Joined");

        // Act
        await host.ExecuteAsync(TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal([3L], host.Output().Rows.Select(row => row[0]));
        Assert.Contains(Assert.Single(source.Queries).Conditions, condition => condition.Field == "Joined" && condition.Operator == DataFilterOperator.GreaterThan);
        Assert.Equal("2026-03-10T00:00:00.0000000Z", host.Run.State.Get<string>(DataPipelineStepContext.GetStateKey("step", DataSourceStep.WatermarkStateKey)));
    }

    [Fact]
    public async Task DataSource_IncrementalDateWithFraction_DoesNotReadTheLastRowAgain()
    {
        // Arrange
        object[][] rows = [[1L, "Ada", new DateTime(2026, 1, 10, 12, 0, 0, 500, DateTimeKind.Utc)]];
        var source = new InMemoryDataSource().Add("People", _people, rows);
        var settings = new DataSourceStepSettings { Source = "Memory", DataSet = "People", IncrementalField = "Joined" };
        using var first = new StepTestHost(new DataSourceStep(new PassThroughStringLocalizer<DataSourceStep>()), settings, Services(source));
        await first.ExecuteAsync(TestContext.Current.CancellationToken);

        using var second = new StepTestHost(new DataSourceStep(new PassThroughStringLocalizer<DataSourceStep>()), settings, Services(source));
        second.Run.State = new DataPipelineState(first.Run.State.ToDictionary());

        // Act
        await second.ExecuteAsync(TestContext.Current.CancellationToken);

        // Assert
        Assert.Single(first.Output().Rows);
        Assert.Empty(second.Output().Rows);
    }

    [Fact]
    public async Task DataSource_IncrementalNothingNew_KeepsTheSavedValue()
    {
        // Arrange
        var source = new InMemoryDataSource().Add("People", _people, _peopleRows);
        var settings = new DataSourceStepSettings { Source = "Memory", DataSet = "People", IncrementalField = "Id" };
        using var host = new StepTestHost(new DataSourceStep(new PassThroughStringLocalizer<DataSourceStep>()), settings, Services(source));
        host.Run.State.Set(DataPipelineStepContext.GetStateKey("step", DataSourceStep.WatermarkStateKey), "3");
        host.Run.State.Set(DataPipelineStepContext.GetStateKey("step", DataSourceStep.WatermarkFieldStateKey), "Memory/People/Id");
        var before = host.Run.State.ToDictionary();

        // Act
        await host.ExecuteAsync(TestContext.Current.CancellationToken);

        // Assert
        Assert.Empty(host.Output().Rows);
        Assert.Equal(before.Keys, host.Run.State.ToDictionary().Keys);
        Assert.Equal("3", host.Run.State.Get<string>(DataPipelineStepContext.GetStateKey("step", DataSourceStep.WatermarkStateKey)));
    }

    [Fact]
    public async Task DataSource_IncrementalFieldChanged_StartsOver()
    {
        // Arrange: the saved value was reached on another field.
        var source = new InMemoryDataSource().Add("People", _people, _peopleRows);
        var settings = new DataSourceStepSettings { Source = "Memory", DataSet = "People", IncrementalField = "Id" };
        using var host = new StepTestHost(new DataSourceStep(new PassThroughStringLocalizer<DataSourceStep>()), settings, Services(source));
        host.Run.State.Set(DataPipelineStepContext.GetStateKey("step", DataSourceStep.WatermarkStateKey), "2026-02-10T00:00:00Z");
        host.Run.State.Set(DataPipelineStepContext.GetStateKey("step", DataSourceStep.WatermarkFieldStateKey), "Memory/People/Joined");

        // Act
        await host.ExecuteAsync(TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(3, host.Output().Rows.Count);
        Assert.Equal("3", host.Run.State.Get<string>(DataPipelineStepContext.GetStateKey("step", DataSourceStep.WatermarkStateKey)));
    }

    [Fact]
    public async Task DataSource_IncrementalPreview_ReadsAsAFirstRunAndKeepsNothing()
    {
        // Arrange
        var source = new InMemoryDataSource().Add("People", _people, _peopleRows);
        var settings = new DataSourceStepSettings { Source = "Memory", DataSet = "People", IncrementalField = "Id" };
        using var host = new StepTestHost(new DataSourceStep(new PassThroughStringLocalizer<DataSourceStep>()), settings, Services(source));
        host.Run.IsPreview = true;
        host.Run.State.Set(DataPipelineStepContext.GetStateKey("step", DataSourceStep.WatermarkStateKey), "3");
        host.Run.State.Set(DataPipelineStepContext.GetStateKey("step", DataSourceStep.WatermarkFieldStateKey), "Memory/People/Id");

        // Act
        await host.ExecuteAsync(TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(3, host.Output().Rows.Count);
        Assert.Equal("3", host.Run.State.Get<string>(DataPipelineStepContext.GetStateKey("step", DataSourceStep.WatermarkStateKey)));
    }

    [Theory]
    [InlineData("Name", 0)]
    [InlineData("Missing", 0)]
    [InlineData("Id", 10)]
    public async Task DataSource_InvalidIncrementalField_ReportsError(string field, int maxRows)
    {
        // Arrange
        var source = new InMemoryDataSource().Add("People", _people, _peopleRows);
        var settings = new DataSourceStepSettings { Source = "Memory", DataSet = "People", IncrementalField = field, MaxRows = maxRows };
        using var host = new StepTestHost(new DataSourceStep(new PassThroughStringLocalizer<DataSourceStep>()), settings, Services(source));

        // Act
        var description = await host.DescribeAsync();

        // Assert
        Assert.Contains(description.Issues, issue => issue.Severity == DataPipelineIssueSeverity.Error);
    }

    [Fact]
    public async Task DataSource_Preview_ReadsPreviewLimitRows()
    {
        // Arrange
        var source = new InMemoryDataSource().Add("People", _people, _peopleRows);
        var settings = new DataSourceStepSettings { Source = "Memory", DataSet = "People" };
        using var host = new StepTestHost(new DataSourceStep(new PassThroughStringLocalizer<DataSourceStep>()), settings, Services(source));
        host.Run.IsPreview = true;
        host.Run.PreviewRowLimit = 2;

        // Act
        await host.ExecuteAsync(TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(2, host.Output().Rows.Count);
        Assert.Equal(2, source.Queries.Single().MaxRows);
    }

    [Fact]
    public async Task DataSource_DataSetTheUserCannotRead_ReportsError()
    {
        // Arrange
        var source = new InMemoryDataSource().Add("People", _people, _peopleRows);
        source.CanRead = _ => false;
        var settings = new DataSourceStepSettings { Source = "Memory", DataSet = "People" };
        using var host = new StepTestHost(new DataSourceStep(new PassThroughStringLocalizer<DataSourceStep>()), settings, Services(source));

        // Act
        var description = await host.DescribeAsync();

        // Assert
        Assert.Contains(description.Issues, issue => issue.Severity == DataPipelineIssueSeverity.Error);
    }

    [Fact]
    public async Task DataSource_UnknownFieldInFilter_ReportsError()
    {
        // Arrange
        var source = new InMemoryDataSource().Add("People", _people, _peopleRows);
        var settings = new DataSourceStepSettings
        {
            Source = "Memory",
            DataSet = "People",
            Filters = [new DataSourceFilter { Field = "Age", Operator = DataFilterOperator.Equals, Values = ["1"] }],
        };
        using var host = new StepTestHost(new DataSourceStep(new PassThroughStringLocalizer<DataSourceStep>()), settings, Services(source));

        // Act
        var description = await host.DescribeAsync();

        // Assert
        Assert.Contains(description.Issues, issue => issue.Message.Contains("Age", StringComparison.Ordinal));
    }

    [Theory]
    [InlineData("Name", DataFilterOperator.GreaterThan)]
    [InlineData("Id", DataFilterOperator.Contains)]
    [InlineData("Joined", DataFilterOperator.StartsWith)]
    [InlineData("Id", DataFilterOperator.InLastDays)]
    public async Task DataSource_FilterOperatorThatDoesNotApplyToTheField_ReportsError(string field, DataFilterOperator filterOperator)
    {
        // Arrange
        var source = new InMemoryDataSource().Add("People", _people, _peopleRows);
        var settings = new DataSourceStepSettings
        {
            Source = "Memory",
            DataSet = "People",
            Filters = [new DataSourceFilter { Field = field, Operator = filterOperator, Values = ["1"] }],
        };
        using var host = new StepTestHost(new DataSourceStep(new PassThroughStringLocalizer<DataSourceStep>()), settings, Services(source));

        // Act
        var description = await host.DescribeAsync();

        // Assert
        var issue = Assert.Single(description.Issues);
        Assert.Equal(DataPipelineIssueSeverity.Error, issue.Severity);
        Assert.Contains(field, issue.Message, StringComparison.Ordinal);
        Assert.Contains(filterOperator.ToString(), issue.Message, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("Name", DataFilterOperator.StartsWith)]
    [InlineData("Id", DataFilterOperator.Between)]
    [InlineData("Joined", DataFilterOperator.InLastDays)]
    [InlineData("Joined", DataFilterOperator.IsNotEmpty)]
    public async Task DataSource_FilterOperatorThatAppliesToTheField_ReportsNoIssue(string field, DataFilterOperator filterOperator)
    {
        // Arrange
        var source = new InMemoryDataSource().Add("People", _people, _peopleRows);
        var settings = new DataSourceStepSettings
        {
            Source = "Memory",
            DataSet = "People",
            Filters = [new DataSourceFilter { Field = field, Operator = filterOperator, Values = ["1"] }],
        };
        using var host = new StepTestHost(new DataSourceStep(new PassThroughStringLocalizer<DataSourceStep>()), settings, Services(source));

        // Act
        var description = await host.DescribeAsync();

        // Assert
        Assert.Empty(description.Issues);
    }

    [Fact]
    public async Task DataSource_MostRowsWithFilterTheSourceIgnores_ReadsUpToMostMatchingRows()
    {
        // Arrange: the source applies the most rows, but not the conditions.
        var source = new InMemoryDataSource().Add("People", _people, _peopleRows);
        var settings = new DataSourceStepSettings
        {
            Source = "Memory",
            DataSet = "People",
            MaxRows = 1,
            Filters = [new DataSourceFilter { Field = "Joined", Operator = DataFilterOperator.GreaterThanOrEqual, Values = ["2026-02-01"] }],
        };
        using var host = new StepTestHost(new DataSourceStep(new PassThroughStringLocalizer<DataSourceStep>()), settings, Services(source));

        // Act
        await host.ExecuteAsync(TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal([[2L, "Bob", new DateTime(2026, 2, 10, 0, 0, 0, DateTimeKind.Utc)]], host.Output().Rows);
    }

    [Fact]
    public async Task DataSource_ParameterPlaceholders_UseTheParametersOfTheRun()
    {
        // Arrange
        var source = new InMemoryDataSource().Add("People", _people, _peopleRows);
        var settings = new DataSourceStepSettings
        {
            Source = "Memory",
            DataSet = "People",
            Fields = ["Name"],
            Parameters = new() { ["Since"] = "{Parameter:Since}" },
            Filters = [new DataSourceFilter { Field = "Joined", Operator = DataFilterOperator.GreaterThanOrEqual, Values = ["{Parameter:Since}"] }],
        };
        using var host = new StepTestHost(new DataSourceStep(new PassThroughStringLocalizer<DataSourceStep>()), settings, Services(source));
        host.Run.Parameters["Since"] = "2026-03-01";

        // Act
        await host.ExecuteAsync(TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal([["Cy"]], host.Output().Rows);
        Assert.Equal("2026-03-01", Assert.Single(source.Queries).Parameters["Since"]);
    }

    [Theory]
    [InlineData(DataFilterOperator.In, new[] { "Ada", "Cy" })]
    [InlineData(DataFilterOperator.NotIn, new[] { "Bob" })]
    public async Task DataSource_ListPlaceholder_GivesEachValueOfTheParameter(DataFilterOperator filterOperator, string[] expected)
    {
        // Arrange
        var source = new InMemoryDataSource().Add("People", _people, _peopleRows);
        var settings = new DataSourceStepSettings
        {
            Source = "Memory",
            DataSet = "People",
            Fields = ["Name"],
            Filters = [new DataSourceFilter { Field = "Name", Operator = filterOperator, Values = ["{Parameter:Names}"] }],
        };
        using var host = new StepTestHost(new DataSourceStep(new PassThroughStringLocalizer<DataSourceStep>()), settings, Services(source));
        host.Run.Parameters["Names"] = "Ada, Cy";

        // Act
        await host.ExecuteAsync(TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(expected, host.Output().Rows.Select(row => (string)row[0]));
    }

    [Fact]
    public async Task DataSource_ListValueWithAComma_IsOneValue()
    {
        // Arrange: a value saved from the editor's "Ada, \"Bob, Jr\"" keeps its comma.
        var source = new InMemoryDataSource().Add("People", _people, _peopleRows);
        var settings = new DataSourceStepSettings
        {
            Source = "Memory",
            DataSet = "People",
            Fields = ["Name"],
            Filters = [new DataSourceFilter { Field = "Name", Operator = DataFilterOperator.In, Values = ["Ada", "Bob, Jr"] }],
        };
        using var host = new StepTestHost(new DataSourceStep(new PassThroughStringLocalizer<DataSourceStep>()), settings, Services(source));

        // Act
        await host.ExecuteAsync(TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal([["Ada"]], host.Output().Rows);
    }

    [Fact]
    public void Render_ParameterPlaceholder_UsesTheParameterOrNothing()
    {
        // Arrange
        var run = new DataPipelineRunContext { PipelineName = "Export" };
        run.Parameters["Region"] = "EMEA";

        // Act
        var text = DataPipelineTemplate.Render("{PipelineName}-{Parameter:Region}-{Parameter:Missing}.", run, DateTime.UtcNow);

        // Assert
        Assert.Equal("Export-EMEA-.", text);
    }

    [Fact]
    public async Task CreateFile_Csv_WritesRowsAndNamesFileFromTemplate()
    {
        // Arrange
        var settings = new CreateFileStepSettings { Format = "csv", FileName = "{PipelineName}-{Date:yyyyMMdd}" };
        using var host = new StepTestHost(new CreateFileStep(new PassThroughStringLocalizer<CreateFileStep>()), settings, Services())
            .WithRows(_people, _peopleRows);

        // Act
        await host.ExecuteAsync(TestContext.Current.CancellationToken);

        // Assert
        var file = Assert.Single(host.Output().Files);
        Assert.Equal("Test pipeline-20260315.csv", file.FileName);
        Assert.Equal("text/csv", file.ContentType);
        Assert.Equal(3, file.RowCount);
        var text = File.ReadAllText(file.Path).TrimStart('﻿');
        Assert.StartsWith("Id,Name,Joined\r\n1,Ada,2026-01-10T00:00:00Z\r\n", text, StringComparison.Ordinal);
    }

    [Fact]
    public async Task CreateFile_NoRows_WritesHeaderFromInputFields()
    {
        // Arrange
        var settings = new CreateFileStepSettings { Format = "xlsx", FileName = "empty" };
        using var host = new StepTestHost(new CreateFileStep(new PassThroughStringLocalizer<CreateFileStep>()), settings, Services())
            .WithRows(_people);

        // Act
        await host.ExecuteAsync(TestContext.Current.CancellationToken);

        // Assert
        var file = Assert.Single(host.Output().Files);
        Assert.Equal("empty.xlsx", file.FileName);
        Assert.Equal(0, file.RowCount);

        await using var stream = file.OpenRead();
        var batches = await DataTestHelpers.ToListAsync(new ExcelDataFileFormat(new PassThroughStringLocalizer<ExcelDataFileFormat>()).ReadAsync(stream, new DataFileOptions(), TestContext.Current.CancellationToken));
        Assert.Equal(["Id", "Name", "Joined"], batches.Single().Fields.Select(field => field.Name));
    }

    [Fact]
    public async Task CreateFile_XlsxWithSeveralConnections_WritesAWorksheetPerConnection()
    {
        // Arrange
        DataField[] totals = [new("Country", "Country", DataFieldType.Text), new("Orders", "Orders", DataFieldType.Integer)];
        var settings = new CreateFileStepSettings { Format = "xlsx", FileName = "report" };
        using var host = new StepTestHost(new CreateFileStep(new PassThroughStringLocalizer<CreateFileStep>()), settings, Services())
            .WithConnection(DataPipelinePort.Input, "People", _people, _peopleRows)
            .WithConnection(DataPipelinePort.Input, "Orders by country", totals, ["CA", 3L], ["FR", 1L]);

        // Act
        await host.ExecuteAsync(TestContext.Current.CancellationToken);

        // Assert
        var file = Assert.Single(host.Output().Files);
        Assert.Equal(5, file.RowCount);

        var people = await ReadSheetAsync(file, "People");
        Assert.Equal(["Id", "Name", "Joined"], people.Fields.Select(field => field.Name));
        Assert.Equal(3, people.Rows.Count);

        var orders = await ReadSheetAsync(file, "Orders by country");
        Assert.Equal(["Country", "Orders"], orders.Fields.Select(field => field.Name));
        Assert.Equal(["CA", "FR"], orders.Rows.Select(row => row[0]));
    }

    [Fact]
    public async Task CreateFile_XlsxWithSheetNames_NamesTheWorksheetsInTheOrderOfTheConnections()
    {
        // Arrange
        var settings = new CreateFileStepSettings { Format = "xlsx", FileName = "report", SheetName = "Everyone\n  \nAgain" };
        using var host = new StepTestHost(new CreateFileStep(new PassThroughStringLocalizer<CreateFileStep>()), settings, Services())
            .WithConnection(DataPipelinePort.Input, "People", _people, _peopleRows)
            .WithConnection(DataPipelinePort.Input, "More people", _people, _peopleRows[0])
            .WithConnection(DataPipelinePort.Input, "Even more people", _people, _peopleRows[1]);

        // Act
        await host.ExecuteAsync(TestContext.Current.CancellationToken);

        // Assert: an empty line keeps the title of its step.
        var file = Assert.Single(host.Output().Files);
        Assert.Equal(3, (await ReadSheetAsync(file, "Everyone")).Rows.Count);
        Assert.Single((await ReadSheetAsync(file, "More people")).Rows);
        Assert.Single((await ReadSheetAsync(file, "Again")).Rows);
    }

    [Fact]
    public async Task CreateFile_CsvWithSeveralConnections_WritesTheRowsOfEveryConnectionByFieldName()
    {
        // Arrange: the second connection has the same fields in another order, and one more.
        DataField[] reordered = [new("Name", "Name", DataFieldType.Text), new("Extra", "Extra", DataFieldType.Text), new("Id", "Id", DataFieldType.Integer)];
        var settings = new CreateFileStepSettings { Format = "csv", FileName = "people" };
        using var host = new StepTestHost(new CreateFileStep(new PassThroughStringLocalizer<CreateFileStep>()), settings, Services())
            .WithConnection(DataPipelinePort.Input, "People", _people, _peopleRows[0])
            .WithConnection(DataPipelinePort.Input, "Others", reordered, ["Zed", "x", 9L]);

        // Act
        await host.ExecuteAsync(TestContext.Current.CancellationToken);

        // Assert
        var file = Assert.Single(host.Output().Files);
        var lines = File.ReadAllText(file.Path).TrimStart('﻿').Split("\r\n", StringSplitOptions.RemoveEmptyEntries);
        Assert.Equal(["Id,Name,Joined", "1,Ada,2026-01-10T00:00:00Z", "9,Zed,"], lines);
    }

    private static async Task<(IReadOnlyList<DataField> Fields, List<object[]> Rows)> ReadSheetAsync(DataPipelineFile file, string sheetName)
    {
        await using var stream = file.OpenRead();
        var batches = await DataTestHelpers.ToListAsync(new ExcelDataFileFormat(new PassThroughStringLocalizer<ExcelDataFileFormat>()).ReadAsync(stream, new DataFileOptions { SheetName = sheetName }, TestContext.Current.CancellationToken));

        return (batches[0].Fields, batches.SelectMany(batch => batch.Rows).ToList());
    }

    [Fact]
    public async Task CreateFile_UnknownFormat_ReportsError()
    {
        // Arrange
        var settings = new CreateFileStepSettings { Format = "pdf" };
        using var host = new StepTestHost(new CreateFileStep(new PassThroughStringLocalizer<CreateFileStep>()), settings, Services())
            .WithRows(_people);

        // Act
        var description = await host.DescribeAsync();

        // Assert
        Assert.Contains(description.Issues, issue => issue.Severity == DataPipelineIssueSeverity.Error);
    }

    [Fact]
    public async Task ZipFiles_SeveralFiles_WritesOneArchive()
    {
        // Arrange
        var settings = new ZipFilesStepSettings { FileName = "exports-{Date:yyyy}" };
        using var host = new StepTestHost(new ZipFilesStep(new PassThroughStringLocalizer<ZipFilesStep>()), settings, Services());
        host.WithFiles(host.CreateFile("a.csv", "A"), host.CreateFile("b.csv", "B"));

        // Act
        await host.ExecuteAsync(TestContext.Current.CancellationToken);

        // Assert
        var file = Assert.Single(host.Output().Files);
        Assert.Equal("exports-2026.zip", file.FileName);
        Assert.Equal("application/zip", file.ContentType);

        using var archive = ZipFile.OpenRead(file.Path);
        Assert.Equal(["a.csv", "b.csv"], archive.Entries.Select(entry => entry.FullName).Order());
    }

    [Theory]
    [InlineData("{PipelineName}", "Test pipeline")]
    [InlineData("report-{Date:yyyy-MM-dd}", "report-2026-03-15")]
    [InlineData("{RunId}", "run")]
    [InlineData("a/b\\c:{Date:HHmm}", "a_b_c_1430")]
    [InlineData("{Unknown}", "{Unknown}")]
    public void Template_Render_ReplacesPlaceholders(string template, string expected)
    {
        // Arrange
        var run = new DataPipelineRunContext
        {
            RunId = "run",
            PipelineName = "Test pipeline",
            StartedUtc = new DateTime(2026, 3, 15, 14, 30, 0, DateTimeKind.Utc),
        };

        // Act
        var name = DataPipelineTemplate.RenderFileName(template, run, run.StartedUtc);

        // Assert
        Assert.Equal(expected, name);
    }

    private static ServiceProvider Services(params InMemoryDataSource[] sources)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddLocalization();
        services.AddDataSourcesCore();

        foreach (var source in sources)
        {
            services.AddSingleton(source);
            services.Configure<DataSourceOptions>(options => options.Add(DataSourceDescriptor.Create<InMemoryDataSource>()));
        }

        return services.BuildServiceProvider();
    }
}
