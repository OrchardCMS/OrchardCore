using Xunit;

namespace OrchardCore.Localization.Extraction.Tests;

public sealed class RazorLocalizationExtractorTests
{
    [Fact]
    public void Extract_ImportsAndLinkedView_PreservesContextAndSourceMapping()
    {
        using var workspace = new TestWorkspace();
        var options = workspace.Options("Module");
        var imports = workspace.Write("Views/_ViewImports.cshtml", "@inject Microsoft.AspNetCore.Mvc.Localization.IViewLocalizer arbitrary");
        var view = workspace.Write("Shared/Physical.cshtml", "<p>@arbitrary[\"A Razor message {0}\", 42]</p>");
        options.RazorFiles.Add(new ExtractionFile(imports, "Views/_ViewImports.cshtml"));
        options.RazorFiles.Add(new ExtractionFile(view, "Views/Linked.cshtml"));
        var catalog = LocalizationExtractor.Extract(options, TestContext.Current.CancellationToken);
        Assert.Empty(catalog.Diagnostics);
        var message = Assert.Single(catalog.Messages);
        Assert.Equal("Module.Views.Linked", message.Context);
        Assert.Equal("A Razor message {0}", message.Text);
        Assert.Equal("Shared/Physical.cshtml", Assert.Single(message.References).Path);
        Assert.Equal(1, message.References[0].Line);
    }

    [Fact]
    public void Extract_RazorPageWithTypedHtmlLocalizer_UsesResourceType()
    {
        using var workspace = new TestWorkspace();
        var options = workspace.Options();
        var resource = workspace.Write("Resources.cs", "namespace Example { public class Resources { } }");
        var page = workspace.Write("Pages/Index.cshtml", """
            @page
            @inject Microsoft.AspNetCore.Mvc.Localization.IHtmlLocalizer<Example.Resources> markup
            <p>@markup["<strong>Page message</strong>"]</p>
            """);
        options.Sources.Add(new ExtractionFile(resource, "Resources.cs"));
        options.RazorFiles.Add(new ExtractionFile(page, "Pages/Index.cshtml"));
        var catalog = LocalizationExtractor.Extract(options, TestContext.Current.CancellationToken);
        Assert.Empty(catalog.Diagnostics);
        var message = Assert.Single(catalog.Messages);
        Assert.Equal("Example.Resources", message.Context);
        Assert.Equal("<strong>Page message</strong>", message.Text);
        Assert.Equal(3, Assert.Single(message.References).Line);
    }

    [Fact]
    public void Extract_PreprocessorSymbols_UsesEvaluatedCompilationOptions()
    {
        using var workspace = new TestWorkspace();
        var options = workspace.Options();
        options.Defines.Add("INCLUDED");
        var path = workspace.Write("Messages.cs", """
            using Microsoft.Extensions.Localization;
            public class Owner
            {
                public string Get(IStringLocalizer<Owner> words)
                {
            #if INCLUDED
                    return words["Active branch"];
            #else
                    return words["Inactive branch"];
            #endif
                }
            }
            """);
        options.Sources.Add(new ExtractionFile(path, "Messages.cs"));
        var catalog = LocalizationExtractor.Extract(options, TestContext.Current.CancellationToken);
        Assert.Empty(catalog.Diagnostics);
        Assert.Equal("Active branch", Assert.Single(catalog.Messages).Text);
    }
}
