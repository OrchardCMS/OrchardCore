using System.Text.Json;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using OrchardCore.DataPipelines.Models;
using OrchardCore.DataPipelines.Services;
using OrchardCore.DataPipelines.Steps;
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

        return context;
    }
}
