using Xunit;

namespace OrchardCore.Localization.Extraction.Tests;

public sealed class DataAnnotationsLocalizationExtractorTests
{
    private const string Context = "OrchardCore.Localization.DataAnnotations.DataAnnotationsDefaultErrorMessages";

    [Fact]
    public void Extract_ValidationAttributes_UsesRuntimeContextAndConstants()
    {
        var catalog = Extract("""
            using System.ComponentModel.DataAnnotations;
            using Mandatory = System.ComponentModel.DataAnnotations.RequiredAttribute;
            public class CustomAttribute : ValidationAttribute { }
            public class Model
            {
                private const string Message = "A value is required.";
                [Mandatory(ErrorMessage = Message)]
                [Custom(ErrorMessage = "Custom failure.")]
                public string Value { get; set; }
                [Compare(nameof(Value), ErrorMessage = "Values must match.")]
                public string Confirmation { get; set; }
            }
            """);
        Assert.Empty(catalog.Diagnostics);
        Assert.Equal(3, catalog.Messages.Count);
        Assert.All(catalog.Messages, message =>
        {
            Assert.Equal(Context, message.Context);
            Assert.Equal("Model.cs", Assert.Single(message.References).Path);
            Assert.True(Assert.Single(message.References).Line > 0);
        });
        Assert.Contains(catalog.Messages, message => message.Text == "A value is required.");
        Assert.Contains(catalog.Messages, message => message.Text == "Custom failure.");
        Assert.Contains(catalog.Messages, message => message.Text == "Values must match.");
        Assert.Empty(catalog.Diagnostics);
    }

    [Fact]
    public void Extract_ExplicitMessages_PreservesMemberAndDisplayNameFallbacks()
    {
        var catalog = Extract("""
            using System.ComponentModel.DataAnnotations;
            public class Model
            {
                [Required(ErrorMessage = "The UserName field is required.")]
                public string UserName { get; set; }
                [Display(Name = "Email address")]
                [Required(ErrorMessage = "Email address is required.")]
                public string Email { get; set; }
            }
            """);
        Assert.Equal(4, catalog.Messages.Count);
        Assert.Contains(catalog.Messages, message => message.Text == "The {0} field is required.");
        Assert.Contains(catalog.Messages, message => message.Text == "{0} is required.");
        Assert.Empty(catalog.Diagnostics);
    }

    [Fact]
    public void Extract_DefaultResourceAndUnrelatedAttributes_DoesNotGuessMessages()
    {
        var catalog = Extract("""
            using System.ComponentModel.DataAnnotations;
            public class OtherAttribute : Attribute { public string ErrorMessage { get; set; } }
            public class Resources { public static string Required => throw new InvalidOperationException(); }
            public class Model
            {
                [Required]
                [StringLength(10)]
                [Other(ErrorMessage = "Not a validation message.")]
                public string Value { get; set; }
                [Required(ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = nameof(Resources.Required))]
                public string ResourceValue { get; set; }
            }
            """);
        Assert.Empty(catalog.Messages);
        Assert.Empty(catalog.Diagnostics);
    }

    [Fact]
    public void Extract_MarkedScopes_SkipsAnnotationsButNotOtherModels()
    {
        var catalog = Extract("""
            using System.ComponentModel.DataAnnotations;
            using OrchardCore.Localization;
            [SkipLocalizationExtraction]
            public class Excluded
            {
                [Required(ErrorMessage = "Excluded.")]
                public string Value { get; set; }
            }
            public class Model
            {
                [SkipLocalizationExtraction]
                public void Forward([Required(ErrorMessage = "Excluded parameter.")] string value) { }
                [Required(ErrorMessage = "Included.")]
                public string Value { get; set; }
            }
            """);
        Assert.Equal("Included.", Assert.Single(catalog.Messages).Text);
        Assert.Empty(catalog.Diagnostics);
    }

    private static LocalizationCatalog Extract(string source)
    {
        using var workspace = new TestWorkspace();
        var options = workspace.Options();
        options.Sources.Add(new ExtractionFile(workspace.Write("Model.cs", source), "Model.cs"));
        options.Sources.Add(new ExtractionFile(workspace.Write("GlobalUsings.cs", "global using System;"), "GlobalUsings.cs"));
        var attributePath = Path.Combine(TestWorkspace.RepositoryRoot, "src", "OrchardCore", "OrchardCore.Abstractions", "Localization", "SkipLocalizationExtractionAttribute.cs");
        options.Sources.Add(new ExtractionFile(attributePath, "SkipLocalizationExtractionAttribute.cs"));
        return LocalizationExtractor.Extract(options, TestContext.Current.CancellationToken);
    }
}
