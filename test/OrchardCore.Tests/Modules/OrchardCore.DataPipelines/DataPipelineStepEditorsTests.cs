using System.Net.Http.Json;
using System.Text.Json;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using OrchardCore.DataPipelines.Ftp;
using OrchardCore.DataPipelines.Models;
using OrchardCore.DataPipelines.Services;
using OrchardCore.DataPipelines.Steps;
using OrchardCore.DataSources;
using OrchardCore.DataSources.Expressions;
using OrchardCore.Entities;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Scope;
using OrchardCore.Tests.Apis.Context;
using OrchardCore.Users;

namespace OrchardCore.Tests.Modules.OrchardCore.DataPipelines;

public sealed class DataPipelineStepEditorsTests
{
    private const string Prefix = "DataPipelineStep.";

    [Fact]
    public async Task Editor_EveryStepType_NamesEveryInputWithTheStepPrefix()
    {
        // Arrange
        using var context = await CreateContextAsync();
        IReadOnlyList<string> stepTypes = null;

        await context.UsingTenantScopeAsync(scope =>
        {
            stepTypes = scope.ServiceProvider.GetRequiredService<IDataPipelineStepTypeManager>().GetStepTypes().Select(stepType => stepType.Name).ToList();

            return Task.CompletedTask;
        });

        var pipelineId = await CreatePipelineAsync(context, stepTypes.Select(type => new DataPipelineStep { StepId = type, Type = type }).ToList());

        foreach (var stepType in stepTypes)
        {
            // Act
            var editor = await GetEditorAsync(context, pipelineId, stepType);

            // Assert
            var names = GetNames(editor).ToList();
            Assert.NotEmpty(names);

            // An editor that fails to render, such as when its view model is sealed, only logs the error: the editor of
            // the step must be there, next to the title every step has. Combining rows has no setting.
            if (stepType != UnionStep.StepName)
            {
                Assert.True(names.Any(name => name != $"{Prefix}Title"), $"The {stepType} editor has no input of its own.");
            }
            // ASP.NET adds an "__Invariant" input next to the number inputs.
            Assert.All(names.Where(name => name != "__Invariant"), name => Assert.True(name.StartsWith(Prefix, StringComparison.Ordinal), $"The input '{name}' of the {stepType} editor isn't bound to the step."));
            Assert.All(editor.QuerySelectorAll("[data-dp-prefix]"), list => Assert.StartsWith(Prefix, list.GetAttribute("data-dp-prefix"), StringComparison.Ordinal));
        }
    }

    [Fact]
    public async Task EditorPost_CalculatedFieldRows_SavesThem()
    {
        // Arrange
        using var context = await CreateContextAsync();
        var pipelineId = await CreatePipelineAsync(context, [new DataPipelineStep { StepId = "calculate", Type = CalculatedFieldsStep.StepName }]);
        // Act
        using var response = await PostEditorAsync(context, pipelineId, "calculate", new()
        {
            [$"{Prefix}Fields[0].Name"] = "Label",
            [$"{Prefix}Fields[0].Formula"] = "UPPER([Name])",
            [$"{Prefix}Fields[1].Name"] = "Total",
            [$"{Prefix}Fields[1].Formula"] = "[Price] * 2",
        });

        // Assert
        response.EnsureSuccessStatusCode();

        await context.UsingTenantScopeAsync(async scope =>
        {
            var pipeline = await scope.ServiceProvider.GetRequiredService<DataPipelineManager>().GetAsync(pipelineId);
            var settings = pipeline.GetEditableDefinition().FindStep("calculate").GetOrCreate<CalculatedFieldsStepSettings>();

            Assert.Equal(["Label", "Total"], settings.Fields.Select(field => field.Name));
            Assert.Equal(["UPPER([Name])", "[Price] * 2"], settings.Fields.Select(field => field.Formula));
        });
    }

    [Theory]
    [InlineData(CalculatedFieldsStep.StepName)]
    [InlineData(FilterStep.StepName)]
    public async Task Editor_FormulaStep_ListsTheRowFunctionsOfTheCatalog(string stepType)
    {
        // Arrange
        using var context = await CreateContextAsync();
        var pipelineId = await CreatePipelineAsync(context, [new DataPipelineStep { StepId = "formula", Type = stepType }]);
        IReadOnlyList<string> rowFunctions = null;

        await context.UsingTenantScopeAsync(scope =>
        {
            rowFunctions = scope.ServiceProvider.GetRequiredService<IExpressionFunctionCatalog>().Functions
                .Where(function => !function.IsAggregate)
                .Select(function => function.Name)
                .ToList();

            return Task.CompletedTask;
        });

        // Act
        var editor = await GetEditorAsync(context, pipelineId, "formula");

        // Assert
        var listed = editor.QuerySelectorAll("[data-dp-insert-function]").Select(item => item.GetAttribute("data-dp-insert-function")).ToList();
        Assert.Equal(rowFunctions, listed);
        Assert.Contains("UPPER", listed);
        Assert.DoesNotContain("SUM", listed);
        Assert.NotNull(editor.QuerySelector("[data-dp-function-search]"));
        Assert.NotNull(editor.QuerySelector("[data-dp-formula-scope]"));
    }

    [Theory]
    [InlineData("csv", "Delimiter", "SheetName")]
    [InlineData("xlsx", "SheetName", "Delimiter")]
    [InlineData("json", "Indented", "SheetName")]
    public async Task Editor_CreateFileFormat_ShowsOnlyItsOptions(string format, string shown, string hidden)
    {
        // Arrange
        using var context = await CreateContextAsync();
        var step = new DataPipelineStep { StepId = "file", Type = CreateFileStep.StepName };
        step.Put(new CreateFileStepSettings { Format = format, Delimiter = ";", SheetName = "Posts" });
        var pipelineId = await CreatePipelineAsync(context, [step]);

        // Act
        var editor = await GetEditorAsync(context, pipelineId, "file");

        // Assert
        Assert.NotNull(editor.QuerySelector($"[name='{Prefix}{shown}']:not([type=hidden])"));

        // The options of the other formats are kept, not shown.
        var kept = Assert.Single(editor.QuerySelectorAll($"[name='{Prefix}{hidden}']"));
        Assert.Equal("hidden", kept.GetAttribute("type"));
    }

    [Fact]
    public async Task EditorPost_CreateFileFormatChanged_ReloadsTheEditor()
    {
        // Arrange
        using var context = await CreateContextAsync();
        var pipelineId = await CreatePipelineAsync(context, [new DataPipelineStep { StepId = "file", Type = CreateFileStep.StepName }]);

        // Act
        using var response = await PostEditorAsync(context, pipelineId, "file", new()
        {
            [$"{Prefix}Format"] = "xlsx",
        });

        // Assert
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        Assert.True(json.RootElement.GetProperty("reloadEditor").GetBoolean());
    }

    [Fact]
    public async Task Editor_DataSourceFilter_NamesTheOperatorsInWords()
    {
        // Arrange
        using var context = await CreateContextAsync();
        var step = new DataPipelineStep { StepId = "source", Type = DataSourceStep.StepName };
        step.Put(new DataSourceStepSettings
        {
            Source = "Contents",
            DataSet = "BlogPost",
            Filters = [new DataSourceFilter { Field = "PublishedUtc", Operator = DataFilterOperator.GreaterThanOrEqual, Values = ["2026-01-01"] }],
        });
        var pipelineId = await CreatePipelineAsync(context, [step]);

        // Act
        var editor = await GetEditorAsync(context, pipelineId, "source");

        // Assert
        var operators = editor.QuerySelectorAll($"select[name='{Prefix}Filters[0].Operator'] option").ToList();
        Assert.Contains(operators, option => option.TextContent == "is greater than or equal to" && option.HasAttribute("selected"));
        Assert.DoesNotContain(operators, option => option.TextContent == option.GetAttribute("value"));
    }

    [Theory]
    [InlineData("DisplayText", DataFilterOperator.Equals, "Text", "StartsWith", "Between")]
    [InlineData("PublishedUtc", DataFilterOperator.Equals, "DateTime", "InLastDays", "Contains")]
    [InlineData("Published", DataFilterOperator.Equals, "Boolean", "IsNotEmpty", "GreaterThan")]
    public async Task Editor_DataSourceFilter_ShowsTheOperatorsOfTheFieldType(string field, DataFilterOperator filterOperator, string fieldType, string shown, string hidden)
    {
        // Arrange
        using var context = await CreateContextAsync();
        var step = new DataPipelineStep { StepId = "source", Type = DataSourceStep.StepName };
        step.Put(new DataSourceStepSettings
        {
            Source = "Contents",
            DataSet = "BlogPost",
            Filters = [new DataSourceFilter { Field = field, Operator = filterOperator }],
        });
        var pipelineId = await CreatePipelineAsync(context, [step]);

        // Act
        var editor = await GetEditorAsync(context, pipelineId, "source");

        // Assert
        var row = editor.QuerySelector("[data-dp-rows] [data-dp-filter-row]");
        Assert.Equal(fieldType, row.QuerySelector($"[data-dp-filter-field] option[value='{field}']").GetAttribute("data-dp-type"));

        var shownOption = row.QuerySelector($"[data-dp-filter-operator] option[value='{shown}']");
        Assert.False(shownOption.HasAttribute("hidden"));
        Assert.False(shownOption.HasAttribute("disabled"));
        Assert.Contains(fieldType, shownOption.GetAttribute("data-dp-types").Split(' '));

        var hiddenOption = row.QuerySelector($"[data-dp-filter-operator] option[value='{hidden}']");
        Assert.True(hiddenOption.HasAttribute("hidden"));
        Assert.True(hiddenOption.HasAttribute("disabled"));
        Assert.DoesNotContain(fieldType, hiddenOption.GetAttribute("data-dp-types").Split(' '));
    }

    [Fact]
    public async Task Editor_DataSourceFilterWithAnOperatorThatDoesNotApply_KeepsItSelected()
    {
        // Arrange: a filter saved before the comparisons were limited to the type of the field.
        using var context = await CreateContextAsync();
        var step = new DataPipelineStep { StepId = "source", Type = DataSourceStep.StepName };
        step.Put(new DataSourceStepSettings
        {
            Source = "Contents",
            DataSet = "BlogPost",
            Filters = [new DataSourceFilter { Field = "PublishedUtc", Operator = DataFilterOperator.Contains, Values = ["2026"] }],
        });
        var pipelineId = await CreatePipelineAsync(context, [step]);

        // Act
        var editor = await GetEditorAsync(context, pipelineId, "source");

        // Assert
        var option = editor.QuerySelector($"select[name='{Prefix}Filters[0].Operator'] option[value='Contains']");
        Assert.True(option.HasAttribute("selected"));
        Assert.False(option.HasAttribute("hidden"));
        Assert.False(option.HasAttribute("disabled"));
    }

    [Fact]
    public async Task Editor_DataSource_LabelsTheFieldsAndTheMaximumNumberOfRows()
    {
        // Arrange
        using var context = await CreateContextAsync();
        var step = new DataPipelineStep { StepId = "source", Type = DataSourceStep.StepName };
        step.Put(new DataSourceStepSettings { Source = "Contents", DataSet = "BlogPost" });
        var pipelineId = await CreatePipelineAsync(context, [step]);

        // Act
        var editor = await GetEditorAsync(context, pipelineId, "source");

        // Assert
        var labels = editor.QuerySelectorAll("label.ocat-label").Select(label => label.TextContent.Trim()).ToList();
        Assert.Contains("Fields to read", labels);
        Assert.Contains("Maximum number of rows", labels);
    }

    [Fact]
    public async Task Editor_DataSource_ShowsTheChosenDataSetAndAsksTheDesignerToChangeIt()
    {
        // Arrange
        using var context = await CreateContextAsync();
        var step = new DataPipelineStep { StepId = "source", Type = DataSourceStep.StepName };
        step.Put(new DataSourceStepSettings { Source = "Contents", DataSet = "BlogPost" });
        var pipelineId = await CreatePipelineAsync(context, [step]);

        // Act
        var editor = await GetEditorAsync(context, pipelineId, "source");

        // Assert
        var picker = editor.QuerySelector("[data-dp-dataset-picker]");
        Assert.Equal("Contents::BlogPost", picker.QuerySelector($"input[type=hidden][name='{Prefix}DataSet']").GetAttribute("value"));
        Assert.Contains("Blog Post", picker.QuerySelector("[data-dp-dataset-summary]").TextContent, StringComparison.Ordinal);
        Assert.Contains("Content items", picker.QuerySelector("[data-dp-dataset-summary]").TextContent, StringComparison.Ordinal);
        Assert.NotNull(picker.QuerySelector("button[data-dp-dataset-open]"));
    }

    [Fact]
    public async Task DataSets_Get_ListsTheDataSetsWithTheirDataSource()
    {
        // Arrange
        using var context = await CreateContextAsync();
        var pipelineId = await CreatePipelineAsync(context, []);

        // Act
        using var response = await context.Client.GetAsync($"Admin/DataPipelines/{pipelineId}/Designer/DataSets", TestContext.Current.CancellationToken);

        // Assert
        response.EnsureSuccessStatusCode();
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        var dataSets = json.RootElement.GetProperty("dataSets").EnumerateArray().ToList();

        var blogPost = Assert.Single(dataSets, dataSet => dataSet.GetProperty("value").GetString() == "Contents::BlogPost");
        Assert.Equal("Blog Post", blogPost.GetProperty("displayName").GetString());
        Assert.Equal("Content items", blogPost.GetProperty("source").GetString());
        Assert.Contains(dataSets, dataSet => dataSet.GetProperty("source").GetString() == "Users");
    }

    [Fact]
    public async Task AddDataSource_DataSet_AddsAStepThatReadsIt()
    {
        // Arrange
        using var context = await CreateContextAsync();
        var pipelineId = await CreatePipelineAsync(context, []);

        // Act
        using var response = await PostJsonAsync(context, pipelineId, "AddDataSource", new { revision = await GetRevisionAsync(context, pipelineId), dataSet = "Contents::BlogPost", x = 80, y = 40 });

        // Assert
        response.EnsureSuccessStatusCode();
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        var stepId = json.RootElement.GetProperty("node").GetProperty("id").GetString();

        await context.UsingTenantScopeAsync(async scope =>
        {
            var pipeline = await scope.ServiceProvider.GetRequiredService<DataPipelineManager>().GetAsync(pipelineId);
            var step = pipeline.GetEditableDefinition().FindStep(stepId);
            var settings = step.GetOrCreate<DataSourceStepSettings>();

            Assert.Equal(DataSourceStep.StepName, step.Type);
            Assert.Equal((80, 40), (step.X, step.Y));
            Assert.Equal(("Contents", "BlogPost"), (settings.Source, settings.DataSet));
        });
    }

    [Theory]
    [InlineData("Contents::Missing")]
    [InlineData("BlogPost")]
    [InlineData(null)]
    public async Task AddDataSource_DataSetTheUserCanNotRead_IsABadRequest(string dataSet)
    {
        // Arrange
        using var context = await CreateContextAsync();
        var pipelineId = await CreatePipelineAsync(context, []);

        // Act
        using var response = await PostJsonAsync(context, pipelineId, "AddDataSource", new { revision = await GetRevisionAsync(context, pipelineId), dataSet, x = 0, y = 0 });

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Theory]
    [InlineData(DataFilterOperator.IsEmpty, new string[0])]
    [InlineData(DataFilterOperator.Equals, new[] { "2026-01-01" })]
    [InlineData(DataFilterOperator.Between, new[] { "2026-01-01", "2026-01-31" })]
    public async Task EditorPost_DataSourceFilter_KeepsTheValuesTheOperatorTakes(DataFilterOperator filterOperator, string[] expected)
    {
        // Arrange
        using var context = await CreateContextAsync();
        var step = new DataPipelineStep { StepId = "source", Type = DataSourceStep.StepName };
        step.Put(new DataSourceStepSettings { Source = "Contents", DataSet = "BlogPost" });
        var pipelineId = await CreatePipelineAsync(context, [step]);

        // Act
        using var response = await PostEditorAsync(context, pipelineId, "source", new()
        {
            [$"{Prefix}DataSet"] = "Contents::BlogPost",
            [$"{Prefix}MaxRows"] = "0",
            [$"{Prefix}Filters[0].Field"] = "PublishedUtc",
            [$"{Prefix}Filters[0].Operator"] = filterOperator.ToString(),
            [$"{Prefix}Filters[0].Value"] = "2026-01-01",
            [$"{Prefix}Filters[0].SecondValue"] = "2026-01-31",
        });

        // Assert
        response.EnsureSuccessStatusCode();

        await context.UsingTenantScopeAsync(async scope =>
        {
            var pipeline = await scope.ServiceProvider.GetRequiredService<DataPipelineManager>().GetAsync(pipelineId);
            var settings = pipeline.GetEditableDefinition().FindStep("source").GetOrCreate<DataSourceStepSettings>();
            var filter = Assert.Single(settings.Filters);

            Assert.Equal(filterOperator, filter.Operator);
            Assert.Equal(expected, filter.Values);
        });
    }

    [Theory]
    [InlineData(DataFilterOperator.In)]
    [InlineData(DataFilterOperator.NotIn)]
    public async Task EditorPost_DataSourceFilterWithAList_SavesEachValueAndShowsThemBack(DataFilterOperator filterOperator)
    {
        // Arrange
        using var context = await CreateContextAsync();
        var step = new DataPipelineStep { StepId = "source", Type = DataSourceStep.StepName };
        step.Put(new DataSourceStepSettings { Source = "Contents", DataSet = "BlogPost" });
        var pipelineId = await CreatePipelineAsync(context, [step]);

        // Act
        using var response = await PostEditorAsync(context, pipelineId, "source", new()
        {
            [$"{Prefix}DataSet"] = "Contents::BlogPost",
            [$"{Prefix}MaxRows"] = "0",
            [$"{Prefix}Filters[0].Field"] = "DisplayText",
            [$"{Prefix}Filters[0].Operator"] = filterOperator.ToString(),
            [$"{Prefix}Filters[0].Value"] = "Man must explore, \"Smith, John\" ,",
        });
        var editor = await GetEditorAsync(context, pipelineId, "source");

        // Assert
        response.EnsureSuccessStatusCode();

        await context.UsingTenantScopeAsync(async scope =>
        {
            var pipeline = await scope.ServiceProvider.GetRequiredService<DataPipelineManager>().GetAsync(pipelineId);
            var settings = pipeline.GetEditableDefinition().FindStep("source").GetOrCreate<DataSourceStepSettings>();

            Assert.Equal(["Man must explore", "Smith, John"], Assert.Single(settings.Filters).Values);
        });

        var value = editor.QuerySelector($"input[name='{Prefix}Filters[0].Value']");
        Assert.Equal("Man must explore, \"Smith, John\"", value.GetAttribute("value"));
    }

    [Fact]
    public async Task Editor_SaveToMediaWithDefaultMediaOptions_WarnsOfTheRejectedFileTypes()
    {
        // Arrange
        using var context = await CreateContextAsync();
        var pipelineId = await CreatePipelineAsync(context, [new DataPipelineStep { StepId = "media", Type = SaveToMediaStep.StepName }]);

        // Act
        var editor = await GetEditorAsync(context, pipelineId, "media");

        // Assert
        var warning = editor.QuerySelector("[data-dp-rejected-extensions]");
        Assert.NotNull(warning);
        Assert.Contains(".csv", warning.TextContent, StringComparison.Ordinal);
        Assert.Contains(".zip", warning.TextContent, StringComparison.Ordinal);
        Assert.DoesNotContain(".xlsx", warning.TextContent, StringComparison.Ordinal);
    }

    [Fact]
    public async Task EditorPost_FtpPassword_SavesItProtected()
    {
        // Arrange
        using var context = await CreateContextAsync();
        var pipelineId = await CreatePipelineAsync(context, [new DataPipelineStep { StepId = "ftp", Type = UploadToFtpStep.StepName }]);

        // Act
        using var response = await PostEditorAsync(context, pipelineId, "ftp", new()
        {
            [$"{Prefix}Host"] = "ftp.example.com",
            [$"{Prefix}Port"] = "21",
            [$"{Prefix}TimeoutSeconds"] = "30",
            [$"{Prefix}Username"] = "exports",
            [$"{Prefix}Password"] = "s3cret!",
        });

        // Assert
        response.EnsureSuccessStatusCode();

        await context.UsingTenantScopeAsync(async scope =>
        {
            var pipeline = await scope.ServiceProvider.GetRequiredService<DataPipelineManager>().GetAsync(pipelineId);
            var settings = pipeline.GetEditableDefinition().FindStep("ftp").GetOrCreate<UploadToFtpStepSettings>();

            Assert.Equal("exports", settings.Username);
            Assert.NotEqual("s3cret!", settings.ProtectedPassword);
            Assert.Equal("s3cret!", scope.ServiceProvider.GetRequiredService<DataPipelineSecrets>().Unprotect(settings.ProtectedPassword));
        });
    }

    // Posts JSON to an endpoint of the designer the way it does: with the antiforgery token of its page in a header.
    private static async Task<HttpResponseMessage> PostJsonAsync(SiteContext context, string pipelineId, string action, object body)
    {
        var (token, cookies) = await GetAntiforgeryAsync(context, pipelineId);

        using var request = new HttpRequestMessage(HttpMethod.Post, $"Admin/DataPipelines/{pipelineId}/Designer/{action}")
        {
            Content = JsonContent.Create(body),
        };
        request.Headers.Add("Cookie", cookies);
        request.Headers.Add("RequestVerificationToken", token);

        return await context.Client.SendAsync(request, TestContext.Current.CancellationToken);
    }

    // Posts a step editor the way the designer does: with the antiforgery token of the designer page.
    private static async Task<HttpResponseMessage> PostEditorAsync(SiteContext context, string pipelineId, string stepId, Dictionary<string, string> values)
    {
        var revision = await GetRevisionAsync(context, pipelineId);
        var (token, cookies) = await GetAntiforgeryAsync(context, pipelineId);
        values["__RequestVerificationToken"] = token;

        using var request = new HttpRequestMessage(HttpMethod.Post, $"Admin/DataPipelines/{pipelineId}/Designer/Editor?stepId={stepId}&revision={revision}")
        {
            Content = new FormUrlEncodedContent(values),
        };
        request.Headers.Add("Cookie", cookies);

        var response = await context.Client.SendAsync(request, TestContext.Current.CancellationToken);
        var body = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        // An invalid form is answered with the editor and its errors.
        Assert.DoesNotContain("\"valid\":false", body, StringComparison.Ordinal);

        return response;
    }

    private static IEnumerable<string> GetNames(IParentNode editor)
    {
        foreach (var element in editor.QuerySelectorAll("[name]"))
        {
            yield return element.GetAttribute("name");
        }

        // The rows the editors script adds are cloned from templates.
        foreach (var template in editor.QuerySelectorAll("template").OfType<IHtmlTemplateElement>())
        {
            foreach (var name in GetNames(template.Content))
            {
                yield return name;
            }
        }
    }

    // The antiforgery token the designer page renders for its POST requests, and the cookie it is paired with.
    private static async Task<(string Token, string Cookies)> GetAntiforgeryAsync(SiteContext context, string pipelineId)
    {
        using var response = await context.Client.GetAsync($"Admin/DataPipelines/{pipelineId}/Edit", TestContext.Current.CancellationToken);
        response.EnsureSuccessStatusCode();

        var document = new HtmlParser().ParseDocument(await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        var token = document.QuerySelector("#data-pipeline-designer-antiforgery input[name=__RequestVerificationToken]").GetAttribute("value");
        var cookies = string.Join("; ", response.Headers.GetValues("Set-Cookie").Select(cookie => cookie.Split(';')[0]));

        return (token, cookies);
    }

    private static async Task<IDocument> GetEditorAsync(SiteContext context, string pipelineId, string stepId)
    {
        using var response = await context.Client.GetAsync($"Admin/DataPipelines/{pipelineId}/Designer/Editor?stepId={stepId}", TestContext.Current.CancellationToken);
        response.EnsureSuccessStatusCode();

        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));

        return new HtmlParser().ParseDocument(json.RootElement.GetProperty("content").GetString());
    }

    private static async Task<string> CreatePipelineAsync(SiteContext context, List<DataPipelineStep> steps)
    {
        string pipelineId = null;

        await context.UsingTenantScopeAsync(async scope =>
        {
            var manager = scope.ServiceProvider.GetRequiredService<DataPipelineManager>();
            var admin = await GetAdminAsync(scope);
            var pipeline = await manager.CreateAsync("Editors", null, admin);
            await manager.SaveDraftAsync(pipeline, pipeline.Revision, draft => draft.Steps = steps, admin);
            pipelineId = pipeline.PipelineId;
        });

        return pipelineId;
    }

    private static async Task<int> GetRevisionAsync(SiteContext context, string pipelineId)
    {
        var revision = 0;

        await context.UsingTenantScopeAsync(async scope =>
        {
            revision = (await scope.ServiceProvider.GetRequiredService<DataPipelineManager>().GetAsync(pipelineId)).Revision;
        });

        return revision;
    }

    private static async Task<ClaimsPrincipal> GetAdminAsync(ShellScope scope)
    {
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IUser>>();
        var factory = scope.ServiceProvider.GetRequiredService<IUserClaimsPrincipalFactory<IUser>>();

        return await factory.CreateAsync(await userManager.FindByNameAsync("admin"));
    }

    private static async Task<BlogContext> CreateContextAsync()
    {
        var context = new BlogContext();
        await context.InitializeAsync();

        await context.UsingTenantScopeAsync(async scope =>
        {
            var featuresManager = scope.ServiceProvider.GetRequiredService<IShellFeaturesManager>();
            var features = (await featuresManager.GetAvailableFeaturesAsync())
                .Where(feature => feature.Id.StartsWith("OrchardCore.DataPipelines", StringComparison.Ordinal))
                .ToList();

            await featuresManager.UpdateFeaturesAsync([], features, force: true);
        });
        await DataPipelineTestSites.WaitForFeaturesAsync(context);

        return context;
    }
}
