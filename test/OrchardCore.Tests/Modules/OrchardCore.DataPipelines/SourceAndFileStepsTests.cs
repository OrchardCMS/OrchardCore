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
        var source = new InMemoryDataSource("Memory").Add("People", _people, _peopleRows);
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
        await host.ExecuteAsync();

        // Assert
        Assert.Equal(["Name", "Id"], description.Outputs[DataPipelinePort.Output].Select(field => field.Name));
        Assert.Equal([["Bob", 2L], ["Cy", 3L]], host.Output().Rows);

        var query = Assert.Single(source.Queries);
        Assert.Same(host.Run.User, query.Context.User);
        Assert.Contains(query.Conditions, condition => condition.Field == "Joined");
    }

    [Fact]
    public async Task DataSource_Preview_ReadsPreviewLimitRows()
    {
        // Arrange
        var source = new InMemoryDataSource("Memory").Add("People", _people, _peopleRows);
        var settings = new DataSourceStepSettings { Source = "Memory", DataSet = "People" };
        using var host = new StepTestHost(new DataSourceStep(new PassThroughStringLocalizer<DataSourceStep>()), settings, Services(source));
        host.Run.IsPreview = true;
        host.Run.PreviewRowLimit = 2;

        // Act
        await host.ExecuteAsync();

        // Assert
        Assert.Equal(2, host.Output().Rows.Count);
        Assert.Equal(2, source.Queries.Single().MaxRows);
    }

    [Fact]
    public async Task DataSource_DataSetTheUserCannotRead_ReportsError()
    {
        // Arrange
        var source = new InMemoryDataSource("Memory").Add("People", _people, _peopleRows);
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
        var source = new InMemoryDataSource("Memory").Add("People", _people, _peopleRows);
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

    [Fact]
    public async Task CreateFile_Csv_WritesRowsAndNamesFileFromTemplate()
    {
        // Arrange
        var settings = new CreateFileStepSettings { Format = "csv", FileName = "{PipelineName}-{Date:yyyyMMdd}" };
        using var host = new StepTestHost(new CreateFileStep(new PassThroughStringLocalizer<CreateFileStep>()), settings, Services())
            .WithRows(_people, _peopleRows);

        // Act
        await host.ExecuteAsync();

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
        await host.ExecuteAsync();

        // Assert
        var file = Assert.Single(host.Output().Files);
        Assert.Equal("empty.xlsx", file.FileName);
        Assert.Equal(0, file.RowCount);

        await using var stream = file.OpenRead();
        var batches = await DataTestHelpers.ToListAsync(new ExcelDataFileFormat(new PassThroughStringLocalizer<ExcelDataFileFormat>()).ReadAsync(stream, new DataFileOptions()));
        Assert.Equal(["Id", "Name", "Joined"], batches.Single().Fields.Select(field => field.Name));
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
        await host.ExecuteAsync();

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

    private static ServiceProvider Services(params IDataSource[] sources)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddLocalization();
        services.AddDataSourcesCore();

        foreach (var source in sources)
        {
            services.AddScoped<IDataSource>(_ => source);
        }

        return services.BuildServiceProvider();
    }
}
