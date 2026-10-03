using System.Text;
using OrchardCore.Localization.PortableObject;
using Xunit;

namespace OrchardCore.Localization.Extraction.Tests;

public sealed class PotWriterTests
{
    [Fact]
    public void Write_Catalog_MatchesDeterministicSnapshot()
    {
        var catalog = new LocalizationCatalog();
        catalog.Add("Z.Context", "One item", "{0} items", new SourceReference("Views/Test.cshtml", 3));
        catalog.Add("A.Context", "Line\n\"quoted\"\\tail\t", null, new SourceReference("Messages.cs", 10), "A translator note.");
        catalog.Add("Z.Context", "One item", "{0} items", new SourceReference("Other.cs", 2));
        var expected = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Snapshots", "Catalog.pot")).Replace("\r\n", "\n", StringComparison.Ordinal);
        Assert.Equal(expected, PotWriter.Write(catalog, "Example"));
        Assert.Empty(catalog.Diagnostics);
    }

    [Fact]
    public void Write_TranslatedTemplate_RoundTripsThroughPortableObjectParser()
    {
        const string text = "Unicode ü 日本語\n\"quoted\"\\tail\t";
        var catalog = new LocalizationCatalog();
        catalog.Add("Example.Context", text, null, new SourceReference("Views/With spaces.liquid", 3));
        catalog.Add("Example.Plurals", "One item", "{0} items", new SourceReference("Messages.cs", 10));
        var template = PotWriter.Write(catalog, "Example");
        using var untranslated = new StringReader(template);
        Assert.Empty(PoParser.Parse(untranslated));

        var translated = template.Replace("msgstr \"\"\n", "msgstr \"Translation\"\n", StringComparison.Ordinal)
            .Replace("msgstr[0] \"\"", "msgstr[0] \"Singular\"", StringComparison.Ordinal)
            .Replace("msgstr[1] \"\"", "msgstr[1] \"Plural\"", StringComparison.Ordinal);
        using var reader = new StringReader(translated);
        var records = PoParser.Parse(reader).ToArray();
        Assert.Equal(2, records.Length);
        Assert.Equal("Example.Context", records[0].Key.Context);
        Assert.Equal(text, records[0].Key.MessageId);
        Assert.Equal(["Translation"], records[0].Translations);
        Assert.Equal("One item", records[1].Key.MessageId);
        Assert.Equal(["Singular", "Plural"], records[1].Translations);
    }

    [Fact]
    public void Add_ConflictingPluralDefinitions_ReportsError()
    {
        var catalog = new LocalizationCatalog();
        catalog.Add("Context", "Key", null, new SourceReference("First.cs"));
        catalog.Add("Context", "Key", "Keys", new SourceReference("Second.cs"));
        Assert.True(Assert.Single(catalog.Diagnostics).IsError);
        Assert.Equal("OCLOC005", catalog.Diagnostics[0].Code);
    }

    [Fact]
    public async Task WriteIfChanged_UnchangedContent_PreservesTimestampAndUtf8()
    {
        using var workspace = new TestWorkspace();
        var path = Path.Combine(workspace.DirectoryPath, "Catalog.pot");
        Assert.True(await PotWriter.WriteIfChangedAsync(path, "Unicode: ü 日本語", TestContext.Current.CancellationToken));
        var timestamp = File.GetLastWriteTimeUtc(path);
        Assert.False(await PotWriter.WriteIfChangedAsync(path, "Unicode: ü 日本語", TestContext.Current.CancellationToken));
        Assert.Equal(timestamp, File.GetLastWriteTimeUtc(path));
        var bytes = await File.ReadAllBytesAsync(path, TestContext.Current.CancellationToken);
        Assert.Equal(Encoding.UTF8.GetBytes("Unicode: ü 日本語"), bytes);
        Assert.Single(Directory.GetFiles(workspace.DirectoryPath));
    }
}
