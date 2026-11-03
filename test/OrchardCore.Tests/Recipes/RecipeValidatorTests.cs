using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using OrchardCore.DataLocalization.Recipes;
using OrchardCore.DisplayManagement.Extensions;
using OrchardCore.Environment.Extensions;
using OrchardCore.Environment.Extensions.Features;
using OrchardCore.Environment.Shell;
using OrchardCore.Features.Recipes.Executors;
using OrchardCore.Media;
using OrchardCore.Media.Recipes;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.RecipeSteps;
using OrchardCore.Recipes.Services;
using OrchardCore.Themes.Recipes;

namespace OrchardCore.Recipes;

public class RecipeValidatorTests
{
    [Fact]
    public async Task Validate_KnownStepInAnyCase_IsValidWithTheNotice()
    {
        var handler = new RecordingHandler("Content");

        var result = await ValidateAsync("""{"steps":[{"name":"content","data":[]}]}""", [handler]);

        Assert.True(result.Valid);
        Assert.Equal(1, result.StepCount);
        Assert.Empty(result.Findings);
        Assert.Contains("doesn't guarantee", result.Notice);
    }

    [Fact]
    public async Task Validate_AnyRecipe_NeverRunsTheHandlers()
    {
        var handler = new RecordingHandler("Content");

        await ValidateAsync("""{"steps":[{"name":"Content"},{"name":"Unknown"}]}""", [handler]);

        Assert.Equal(0, handler.Executions);
    }

    [Fact]
    public async Task Validate_UnknownStep_ReportsItsIndexAndName()
    {
        var result = await ValidateAsync("""{"steps":[{"name":"Content"},{"name":"Contnet"}]}""", [new RecordingHandler("Content")]);

        Assert.False(result.Valid);
        Assert.Equal(1, result.ErrorCount);
        var finding = Assert.Single(result.Findings);
        Assert.Equal((RecipeValidationSeverity.Error, "unknown-step", 1, "Contnet"), (finding.Severity, finding.Code, finding.StepIndex, finding.StepName));
        Assert.Contains("still succeed", finding.Message);
    }

    [Fact]
    public async Task Validate_UnknownStepAfterAFeatureIsEnabled_OnlyWarns()
    {
        var features = new FeatureStepValidator(CreateFeatures(available: ["A"]), new PassThroughLocalizer<FeatureStepValidator>());

        var result = await ValidateAsync("""{"steps":[{"name":"Feature","enable":["A"]},{"name":"FromA"}]}""", [new RecordingHandler("Feature")], [features]);

        Assert.True(result.Valid);
        var finding = Assert.Single(result.Findings);
        Assert.Equal((RecipeValidationSeverity.Warning, "step-handler-unverified", 1), (finding.Severity, finding.Code, finding.StepIndex));
    }

    [Fact]
    public async Task Validate_UnknownStepBeforeAFeatureIsEnabled_IsAnError()
    {
        var features = new FeatureStepValidator(CreateFeatures(available: ["A"]), new PassThroughLocalizer<FeatureStepValidator>());

        var result = await ValidateAsync("""{"steps":[{"name":"FromA"},{"name":"Feature","enable":["A"]}]}""", [new RecordingHandler("Feature")], [features]);

        Assert.Equal("unknown-step", Assert.Single(result.Findings).Code);
    }

    [Fact]
    public async Task Validate_UnknownStepAfterEnablingAnEnabledFeature_IsAnError()
    {
        var features = new FeatureStepValidator(CreateFeatures(available: ["A"], enabled: ["A"]), new PassThroughLocalizer<FeatureStepValidator>());

        var result = await ValidateAsync("""{"steps":[{"name":"Feature","enable":["A"]},{"name":"FromA"}]}""", [new RecordingHandler("Feature")], [features]);

        Assert.Equal("unknown-step", Assert.Single(result.Findings).Code);
    }

    [Fact]
    public async Task Validate_UnknownStepAfterACommand_OnlyWarns()
    {
        var result = await ValidateAsync("""{"steps":[{"name":"Command","commands":["feature enable A"]},{"name":"FromA"}]}""",
            [new RecordingHandler("Command")], [new CommandStepValidator(new PassThroughLocalizer<CommandStepValidator>())]);

        Assert.True(result.Valid);
        Assert.Equal(["commands-not-validated", "step-handler-unverified"], result.Findings.Select(finding => finding.Code));
    }

    [Fact]
    public async Task Validate_HandlerNotDeclaringItsSteps_OnlyWarnsForUnknownStep()
    {
        var result = await ValidateAsync("""{"steps":[{"name":"Custom"}]}""", [new UndeclaredHandler()]);

        Assert.True(result.Valid);
        Assert.Equal(1, result.WarningCount);
        Assert.Equal("step-handler-unverified", Assert.Single(result.Findings).Code);
    }

    [Theory]
    [InlineData("""{"steps":""", "recipe-invalid-json")]
    [InlineData("""[]""", "recipe-not-object")]
    [InlineData("""{"name":"NoSteps"}""", "recipe-no-steps")]
    [InlineData("""{"steps":{}}""", "recipe-no-steps")]
    public async Task Validate_InvalidRecipe_ReportsAFindingWithoutAStep(string json, string code)
    {
        var result = await ValidateAsync(json, []);

        Assert.False(result.Valid);
        Assert.Equal(0, result.StepCount);
        var finding = Assert.Single(result.Findings);
        Assert.Equal(code, finding.Code);
        Assert.Null(finding.StepIndex);
        Assert.Null(finding.StepName);
    }

    [Fact]
    public async Task Validate_VariablesThatAreNotAnObject_IsAnError()
    {
        var result = await ValidateAsync("""{"variables":[],"steps":[]}""", []);

        Assert.Equal("recipe-invalid-variables", Assert.Single(result.Findings).Code);
    }

    [Theory]
    [InlineData("""{"steps":[1]}""", "step-not-object")]
    [InlineData("""{"steps":[{}]}""", "step-no-name")]
    [InlineData("""{"steps":[{"name":1}]}""", "step-no-name")]
    [InlineData("""{"steps":[{"Name":"Content"}]}""", "step-no-name")]
    [InlineData("""{"steps":[{"name":" "}]}""", "step-no-name")]
    public async Task Validate_InvalidStep_ReportsItsIndex(string json, string code)
    {
        var result = await ValidateAsync(json, [new RecordingHandler("Content")]);

        Assert.False(result.Valid);
        Assert.Equal(1, result.StepCount);
        var finding = Assert.Single(result.Findings);
        Assert.Equal(code, finding.Code);
        Assert.Equal(0, finding.StepIndex);
    }

    [Fact]
    public async Task Validate_CommentsAndTrailingCommas_AreAcceptedLikeTheExecutor()
    {
        var result = await ValidateAsync("""
            {
              // A comment.
              "steps": [{"name":"Content"},],
            }
            """, [new RecordingHandler("Content")]);

        Assert.True(result.Valid);
    }

    [Fact]
    public async Task Validate_RecipeFileCantBeRead_IsAnError()
    {
        var fileInfo = new Mock<IFileInfo>();
        fileInfo.Setup(value => value.CreateReadStream()).Throws(new FileNotFoundException("private path"));
        var validator = CreateValidator([], []);

        var result = await validator.ValidateAsync(new RecipeDescriptor { RecipeFileInfo = fileInfo.Object }, TestContext.Current.CancellationToken);

        var finding = Assert.Single(result.Findings);
        Assert.Equal("recipe-unreadable", finding.Code);
        Assert.DoesNotContain("private path", finding.Message);
    }

    [Fact]
    public async Task Validate_ScriptsAnywhereInAStep_AreCountedAndNeverEvaluated()
    {
        var result = await ValidateAsync("""{"steps":[{"name":"Content","a":"[js:throw 1]","b":[{"c":"[liquid:{{ x }}]"}],"d":"[not a script]"}]}""", [new RecordingHandler("Content")]);

        Assert.True(result.Valid);
        var finding = Assert.Single(result.Findings);
        Assert.Equal((RecipeValidationSeverity.Info, "scripts-not-evaluated"), (finding.Severity, finding.Code));
        Assert.Contains("2 script", finding.Message);
    }

    [Theory]
    [InlineData("[file:text('present.txt')]", null)]
    [InlineData("[file:base64('sub/present.txt')]", null)]
    [InlineData("[file:text('sub/../present.txt')]", null)]
    [InlineData("[file:text('missing.txt')]", "file-not-found")]
    [InlineData("[file:text('../outside.txt')]", "file-not-found")]
    [InlineData("[file:text('sub/../../outside.txt')]", "file-not-found")]
    [InlineData("[file:unknown('present.txt')]", "file-script-invalid")]
    [InlineData("[file:text('')]", "file-script-invalid")]
    public async Task Validate_FileScript_ResolvesInsideTheRecipeFiles(string script, string code)
    {
        var root = Directory.CreateTempSubdirectory().FullName;
        try
        {
            var files = Directory.CreateDirectory(Path.Combine(root, "package")).FullName;
            Directory.CreateDirectory(Path.Combine(files, "sub"));
            await File.WriteAllTextAsync(Path.Combine(files, "present.txt"), "x", TestContext.Current.CancellationToken);
            await File.WriteAllTextAsync(Path.Combine(files, "sub", "present.txt"), "x", TestContext.Current.CancellationToken);
            await File.WriteAllTextAsync(Path.Combine(root, "outside.txt"), "x", TestContext.Current.CancellationToken);
            using var provider = new PhysicalFileProvider(files);

            var result = await CreateValidator([new RecordingHandler("Content")], []).ValidateAsync(
                CreateRecipe(JsonSerializer.Serialize(new { steps = new[] { new { name = "Content", data = script } } }), provider),
                TestContext.Current.CancellationToken);

            if (code is null)
            {
                Assert.Empty(result.Findings);
            }
            else
            {
                Assert.False(result.Valid);
                Assert.Equal(code, Assert.Single(result.Findings).Code);
            }
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task Validate_StepOfAnotherTypeThanItsValidatorExpects_IsAnError()
    {
        var features = new FeatureStepValidator(CreateFeatures(available: []), new PassThroughLocalizer<FeatureStepValidator>());

        var result = await ValidateAsync("""{"steps":[{"name":"Feature","enable":"A"}]}""", [new RecordingHandler("Feature")], [features]);

        Assert.Equal("step-invalid", Assert.Single(result.Findings).Code);
    }

    [Fact]
    public async Task Validate_ValidatorOfAnotherStep_IsNotRun()
    {
        var other = new Mock<IRecipeStepValidator>(MockBehavior.Strict);
        other.SetupGet(value => value.StepName).Returns("Other");

        var result = await ValidateAsync("""{"steps":[{"name":"Content"}]}""", [new RecordingHandler("Content")], [other.Object]);

        Assert.True(result.Valid);
    }

    [Fact]
    public void CreateFailure_AnyMessage_IsInvalidWithTheNotice()
    {
        var result = CreateValidator([], []).CreateFailure("package-invalid", "Bad package.");

        Assert.False(result.Valid);
        Assert.Equal(1, result.ErrorCount);
        Assert.Contains("doesn't guarantee", result.Notice);
        Assert.Equal(("package-invalid", "Bad package."), (result.Findings[0].Code, result.Findings[0].Message));
    }

    [Fact]
    public void StepNames_NamedHandlerAndHandlerOfTwoSteps_DeclareThem()
    {
        Assert.Equal(["Content"], ((IRecipeStepHandler)new RecordingHandler("Content")).StepNames);
        Assert.Equal(["Translations", "DynamicDataTranslations"], ((IRecipeStepHandler)new TranslationsStep(null, new PassThroughLocalizer<TranslationsStep>())).StepNames);
        Assert.Null(((IRecipeStepHandler)new UndeclaredHandler()).StepNames);
    }

    [Theory]
    [InlineData("[js:uuid()]", true)]
    [InlineData("[file:text('a')]", true)]
    [InlineData("[locale en]Text[/locale]", false)]
    [InlineData("[1js:uuid()]", false)]
    [InlineData("js:uuid()", false)]
    [InlineData("[:x]", false)]
    [InlineData(null, false)]
    public void IsScript_Value_MatchesWhatTheExecutorEvaluates(string value, bool expected)
    {
        Assert.Equal(expected, RecipeValidationContext.IsScript(value));
    }

    [Fact]
    public async Task FeatureStepValidator_UnknownFeatureToEnable_IsAnError()
    {
        var context = await ValidateFeatureStepAsync("""{"enable":["Missing"]}""", available: ["A"]);

        var finding = Assert.Single(context.Findings);
        Assert.Equal((RecipeValidationSeverity.Error, "feature-not-found"), (finding.Severity, finding.Code));
    }

    [Fact]
    public async Task FeatureStepValidator_FeatureIdInAnotherCase_IsAnErrorLikeTheStepIgnoresIt()
    {
        var context = await ValidateFeatureStepAsync("""{"enable":["a"]}""", available: ["A"]);

        Assert.Equal("feature-not-found", Assert.Single(context.Findings).Code);
    }

    [Fact]
    public async Task FeatureStepValidator_UnknownFeatureToDisable_OnlyWarns()
    {
        var context = await ValidateFeatureStepAsync("""{"disable":["Missing"]}""", available: ["A"]);

        var finding = Assert.Single(context.Findings);
        Assert.Equal((RecipeValidationSeverity.Warning, "feature-not-found"), (finding.Severity, finding.Code));
    }

    [Fact]
    public async Task FeatureStepValidator_DisabledAndEnabledFeatures_OnlyTheDisabledOneIsPending()
    {
        var context = await ValidateFeatureStepAsync("""{"enable":["A","B"]}""", available: ["A", "B"], enabled: ["B"]);

        Assert.Empty(context.Findings);
        Assert.Equal(["A"], context.PendingFeatures);
    }

    [Fact]
    public async Task FeatureStepValidator_FeatureFromAScript_WarnsAndFeaturesMayChange()
    {
        var context = await ValidateFeatureStepAsync("""{"enable":["[js:parameters('feature')]"]}""", available: ["A"]);

        Assert.Equal("feature-not-evaluated", Assert.Single(context.Findings).Code);
        Assert.True(context.FeaturesMayChange);
    }

    [Fact]
    public async Task ThemesStepValidator_MissingThemeAndModule_AreErrors()
    {
        var context = await ValidateThemesStepAsync("""{"site":"Missing","admin":"Module"}""");

        Assert.Equal(["theme-not-found", "theme-not-found"], context.Findings.Select(finding => finding.Code));
    }

    [Fact]
    public async Task ThemesStepValidator_DisabledTheme_WarnsUnlessAnEarlierStepEnablesIt()
    {
        var warned = await ValidateThemesStepAsync("""{"site":"Theme"}""");
        var pending = await ValidateThemesStepAsync("""{"site":"Theme"}""", context => context.PendingFeatures.Add("Theme"));

        Assert.Equal("theme-not-enabled", Assert.Single(warned.Findings).Code);
        Assert.Empty(pending.Findings);
    }

    [Fact]
    public async Task ThemesStepValidator_ThemeFromAScript_OnlyWarns()
    {
        var context = await ValidateThemesStepAsync("""{"site":"[js:parameters('theme')]"}""");

        Assert.Equal("theme-not-evaluated", Assert.Single(context.Findings).Code);
    }

    [Theory]
    [InlineData("""{"Files":[{"TargetPath":"a.txt","SourcePath":"present.txt"}]}""", null)]
    [InlineData("""{"Files":[{"Path":"a.txt","Base64":"eA=="}]}""", null)]
    [InlineData("""{"Files":[{"TargetPath":"a.txt","SourcePath":"[js:x]"}]}""", null)]
    [InlineData("""{"Files":[{"TargetPath":"a.txt","SourcePath":"missing.txt"}]}""", "file-not-found")]
    [InlineData("""{"Files":[{"TargetPath":"a.txt","SourcePath":"../outside.txt"}]}""", "file-not-found")]
    [InlineData("""{"Files":[{"TargetPath":"../a.txt","Base64":"eA=="}]}""", "media-target-invalid")]
    [InlineData("""{"Files":[{"TargetPath":"a.exe","Base64":"eA=="}]}""", "media-extension-not-allowed")]
    [InlineData("""{"Files":[{"TargetPath":"a.txt"}]}""", "media-no-source")]
    [InlineData("""{"Files":[{"TargetPath":"a.txt","SourceUrl":"https://example.com/a.txt"}]}""", "media-url-not-fetched")]
    [InlineData("""{"name":"media"}""", "media-no-files")]
    public async Task MediaStepValidator_File_AppliesTheRulesOfTheStep(string json, string code)
    {
        var root = Directory.CreateTempSubdirectory().FullName;
        try
        {
            var files = Directory.CreateDirectory(Path.Combine(root, "package")).FullName;
            await File.WriteAllTextAsync(Path.Combine(files, "present.txt"), "x", TestContext.Current.CancellationToken);
            await File.WriteAllTextAsync(Path.Combine(root, "outside.txt"), "x", TestContext.Current.CancellationToken);
            using var provider = new PhysicalFileProvider(files);
            var options = Options.Create(new MediaOptions { AllowedFileExtensions = [".txt"], RestrictedFileExtensions = [] });
            var context = new RecipeValidationContext
            {
                Recipe = new RecipeDescriptor { FileProvider = provider, BasePath = "" },
                Step = JsonNode.Parse(json).AsObject(),
            };

            await new MediaStepValidator(options, new PassThroughLocalizer<MediaStepValidator>()).ValidateAsync(context);

            Assert.Equal(code is null ? Array.Empty<string>() : [code], context.Findings.Select(finding => finding.Code));
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Theory]
    [InlineData("""{"Values":[{"name":"Present"}]}""", null)]
    [InlineData("""{"Values":[{"name":"Missing"}]}""", "recipe-not-found")]
    [InlineData("""{"name":"Recipes"}""", "recipes-no-values")]
    public async Task RecipesStepValidator_InnerRecipe_IsLookedUpAndFeaturesMayChange(string json, string code)
    {
        var harvester = new Mock<IRecipeHarvester>();
        harvester.Setup(value => value.HarvestRecipesAsync()).ReturnsAsync([new RecipeDescriptor { Name = "Present" }]);
        var context = new RecipeValidationContext { Step = JsonNode.Parse(json).AsObject() };

        await new RecipesStepValidator([harvester.Object], new RecipeDescriptorCache(), new PassThroughLocalizer<RecipesStepValidator>()).ValidateAsync(context);

        Assert.True(context.FeaturesMayChange);
        if (code is null)
        {
            Assert.Equal("inner-recipes-not-validated", Assert.Single(context.Findings).Code);
        }
        else
        {
            Assert.Contains(context.Findings, finding => finding.Code == code && finding.Severity == RecipeValidationSeverity.Error);
        }
    }

    private static async Task<RecipeValidationContext> ValidateFeatureStepAsync(string json, string[] available, string[] enabled = null)
    {
        var context = new RecipeValidationContext { Step = JsonNode.Parse(json).AsObject() };
        await new FeatureStepValidator(CreateFeatures(available, enabled), new PassThroughLocalizer<FeatureStepValidator>()).ValidateAsync(context);

        return context;
    }

    private static async Task<RecipeValidationContext> ValidateThemesStepAsync(string json, Action<RecipeValidationContext> prepare = null)
    {
        var theme = new Mock<IThemeExtensionInfo>();
        theme.SetupGet(value => value.Id).Returns("Theme");
        var themeFeature = new Mock<IFeatureInfo>();
        themeFeature.SetupGet(value => value.Id).Returns("Theme");
        themeFeature.SetupGet(value => value.Extension).Returns(theme.Object);
        var moduleFeature = new Mock<IFeatureInfo>();
        moduleFeature.SetupGet(value => value.Id).Returns("Module");
        moduleFeature.SetupGet(value => value.Extension).Returns(Mock.Of<IExtensionInfo>());
        var features = new Mock<IShellFeaturesManager>();
        features.Setup(value => value.GetAvailableFeaturesAsync()).ReturnsAsync([themeFeature.Object, moduleFeature.Object]);
        features.Setup(value => value.GetEnabledFeaturesAsync()).ReturnsAsync([moduleFeature.Object]);

        var context = new RecipeValidationContext { Step = JsonNode.Parse(json).AsObject() };
        prepare?.Invoke(context);
        await new ThemesStepValidator(features.Object, new PassThroughLocalizer<ThemesStepValidator>()).ValidateAsync(context);

        return context;
    }

    private static IShellFeaturesManager CreateFeatures(string[] available, string[] enabled = null)
    {
        var features = new Mock<IShellFeaturesManager>();
        features.Setup(value => value.GetAvailableFeaturesAsync()).ReturnsAsync(available.Select(CreateFeature).ToArray());
        features.Setup(value => value.GetEnabledFeaturesAsync()).ReturnsAsync((enabled ?? []).Select(CreateFeature).ToArray());

        return features.Object;
    }

    private static IFeatureInfo CreateFeature(string id)
    {
        var feature = new Mock<IFeatureInfo>();
        feature.SetupGet(value => value.Id).Returns(id);

        return feature.Object;
    }

    private static Task<RecipeValidationResult> ValidateAsync(string json, IRecipeStepHandler[] handlers, IRecipeStepValidator[] validators = null)
        => CreateValidator(handlers, validators ?? []).ValidateAsync(CreateRecipe(json, new NullFileProvider()), TestContext.Current.CancellationToken);

    private static RecipeValidator CreateValidator(IRecipeStepHandler[] handlers, IRecipeStepValidator[] validators)
        => new(handlers, validators, new PassThroughLocalizer<RecipeValidator>());

    private static RecipeDescriptor CreateRecipe(string json, IFileProvider provider) => new()
    {
        BasePath = "",
        FileProvider = provider,
        RecipeFileInfo = new TextFileInfo(json),
    };

    private sealed class RecordingHandler : NamedRecipeStepHandler
    {
        public RecordingHandler(string stepName)
            : base(stepName)
        {
        }

        public int Executions { get; private set; }

        protected override Task HandleAsync(RecipeExecutionContext context)
        {
            Executions++;

            return Task.CompletedTask;
        }
    }

    private sealed class UndeclaredHandler : IRecipeStepHandler
    {
        public Task ExecuteAsync(RecipeExecutionContext context) => Task.CompletedTask;
    }

    private sealed class PassThroughLocalizer<T> : IStringLocalizer<T>
    {
        public LocalizedString this[string name] => new(name, name);

        public LocalizedString this[string name, params object[] arguments] => new(name, string.Format(name, arguments));

        public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures) => [];
    }

    private sealed class TextFileInfo : IFileInfo
    {
        private readonly string _text;

        public TextFileInfo(string text)
        {
            _text = text;
        }

        public bool Exists => true;

        public bool IsDirectory => false;

        public DateTimeOffset LastModified => DateTimeOffset.UtcNow;

        public long Length => Encoding.UTF8.GetByteCount(_text);

        public string Name => "Recipe.json";

        public string PhysicalPath => null;

        public Stream CreateReadStream() => new MemoryStream(Encoding.UTF8.GetBytes(_text));
    }
}
