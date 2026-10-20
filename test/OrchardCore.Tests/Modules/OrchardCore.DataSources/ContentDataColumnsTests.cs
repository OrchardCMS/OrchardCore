using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.DataSources;
using OrchardCore.DataSources.Contents;

namespace OrchardCore.Tests.Modules.OrchardCore.DataSources;

public sealed class ContentDataColumnsTests
{
    [Fact]
    public void Build_PartWrittenInCode_ReadsItsPropertiesTypedByTheirTypes()
    {
        // Arrange
        var definition = CreateDefinition();

        // Act
        var fields = Build(definition).ToDictionary(column => column.Field.Name, column => column.Field);

        // Assert
        Assert.Equal(DataFieldType.Text, fields["DispatchPart.Code"].Type);
        Assert.Equal(DataFieldType.Integer, fields["DispatchPart.Rank"].Type);
        Assert.Equal(DataFieldType.Decimal, fields["DispatchPart.Weight"].Type);
        Assert.Equal(DataFieldType.Boolean, fields["DispatchPart.Urgent"].Type);
        Assert.Equal(DataFieldType.DateTime, fields["DispatchPart.DueUtc"].Type);
        Assert.Equal(DataFieldType.Text, fields["DispatchPart.Status"].Type);
        Assert.Equal(DataFieldType.Text, fields["DispatchPart.Tags"].Type);
        Assert.Equal(DataFieldType.Text, fields["DispatchPart.Settings.Mode"].Type);
        Assert.Equal(DataFieldType.Integer, fields["DispatchPart.Settings.Limits.Retries"].Type);
        Assert.Equal("Dispatch Settings mode", fields["DispatchPart.Settings.Mode"].DisplayName);
        Assert.Equal("Dispatch", fields["DispatchPart.Settings.Mode"].Group);
    }

    [Fact]
    public void Build_PartWrittenInCode_LeavesOutSecretsIgnoredPropertiesAndListsOfObjects()
    {
        // Arrange
        var definition = CreateDefinition();

        // Act
        var names = Build(definition).Select(column => column.Field.Name).ToList();

        // Assert
        Assert.DoesNotContain("DispatchPart.Password", names);
        Assert.DoesNotContain("DispatchPart.ApiKey", names);
        Assert.DoesNotContain("DispatchPart.RefreshToken", names);
        Assert.DoesNotContain("DispatchPart.Hidden", names);
        Assert.DoesNotContain("DispatchPart.Stops", names);
        Assert.DoesNotContain("DispatchPart.Labels", names);
        Assert.DoesNotContain(names, name => name.StartsWith("DispatchPart.Raw", StringComparison.Ordinal));
    }

    [Fact]
    public void Build_FieldsOfParts_ReadsCodeFieldsDefinedFieldsAndFieldsOfAdminParts()
    {
        // Arrange
        var definition = CreateDefinition();

        // Act
        var fields = Build(definition).ToDictionary(column => column.Field.Name, column => column.Field);

        // Assert
        // A content field declared as a property of the part class.
        Assert.Equal(DataFieldType.Text, fields["DispatchPart.Summary"].Type);
        Assert.Equal("Summary", fields["DispatchPart.Summary"].DisplayName);

        // A field type written in code, with two values, read from its class.
        Assert.Equal(DataFieldType.Decimal, fields["DispatchPart.Location.Latitude"].Type);
        Assert.Equal(DataFieldType.Decimal, fields["DispatchPart.Location.Longitude"].Type);

        // A part created in the admin, which has no class: its fields come from its definition.
        Assert.Equal(DataFieldType.Text, fields["Dispatch.Notes"].Type);
        Assert.Equal(DataFieldType.Decimal, fields["Dispatch.Cost"].Type);

        // A field type that is neither described nor registered has nothing to read.
        Assert.DoesNotContain(fields.Keys, name => name.StartsWith("Dispatch.Unknown", StringComparison.Ordinal));
    }

    [Fact]
    public void Build_DescribedValues_ComeFirstAndHideThePropertiesOfTheSameName()
    {
        // Arrange
        var definition = CreateDefinition();
        var options = new ContentDataSourceOptions();
        options.Parts["DispatchPart"] = [new("Code", DataFieldType.Integer)];
        options.Fields["GeoPointField"] = [new("Latitude", DataFieldType.Decimal)];

        // Act
        var columns = ContentDataColumns.Build(definition, options, CreateContentOptions(), new PassThroughStringLocalizer<ContentDataColumnsTests>());
        var names = columns.Select(column => column.Field.Name).ToList();

        // Assert
        var code = Assert.Single(columns, column => column.Field.Name == "DispatchPart.Code");
        Assert.Equal(DataFieldType.Integer, code.Field.Type);
        Assert.True(names.IndexOf("DispatchPart.Code") < names.IndexOf("DispatchPart.Rank"));

        // A described field type is read only as described, so its single value is named after the field.
        Assert.Contains("DispatchPart.Location", names);
        Assert.DoesNotContain("DispatchPart.Location.Longitude", names);
    }

    [Fact]
    public void Build_WithoutContentOptions_ReadsDescribedValuesOnly()
    {
        // Arrange
        var definition = CreateDefinition();

        // Act
        var names = ContentDataColumns.Build(definition, new ContentDataSourceOptions(), null, new PassThroughStringLocalizer<ContentDataColumnsTests>())
            .Select(column => column.Field.Name)
            .ToList();

        // Assert
        Assert.DoesNotContain("DispatchPart.Rank", names);
        Assert.Contains("Dispatch.Notes", names);
    }

    [Fact]
    public void WriteAndRead_PropertiesOfPartWrittenInCode_RoundTripThroughThePartClass()
    {
        // Arrange
        var columns = Build(CreateDefinition()).ToDictionary(column => column.Field.Name);
        var item = new ContentItem { ContentType = "Dispatch" };
        var due = new DateTime(2026, 5, 4, 12, 30, 0, DateTimeKind.Utc);

        // Act
        columns["DispatchPart.Code"].Write(item, "A-12");
        columns["DispatchPart.Rank"].Write(item, 3L);
        columns["DispatchPart.DueUtc"].Write(item, due);
        columns["DispatchPart.Status"].Write(item, "Shipped");
        columns["DispatchPart.Tags"].Write(item, "red, blue");
        columns["DispatchPart.Settings.Mode"].Write(item, "Fast");
        columns["DispatchPart.Settings.Limits.Retries"].Write(item, 5L);
        columns["DispatchPart.Summary"].Write(item, "Two boxes");
        columns["DispatchPart.Location.Latitude"].Write(item, 45.5m);

        // Assert
        Assert.True(item.TryGet<DispatchPart>(out var part));
        Assert.Equal("A-12", part.Code);
        Assert.Equal(3, part.Rank);
        Assert.Equal(due, part.DueUtc);
        Assert.Equal(DispatchStatus.Shipped, part.Status);
        Assert.Equal(["red", "blue"], part.Tags);
        Assert.Equal("Fast", part.Settings.Mode);
        Assert.Equal(5, part.Settings.Limits.Retries);
        Assert.Equal("Two boxes", part.Summary.Text);
        Assert.Equal(45.5m, part.Get<GeoPointField>("Location").Latitude);

        Assert.Equal("Fast", columns["DispatchPart.Settings.Mode"].Read(item));
        Assert.Equal(5L, columns["DispatchPart.Settings.Limits.Retries"].Read(item));
        Assert.Equal("red,blue", columns["DispatchPart.Tags"].Read(item));
        Assert.Equal(45.5m, columns["DispatchPart.Location.Latitude"].Read(item));
    }

    [Fact]
    public void Build_ValuesThatCantBeWrittenBack_AreReadOnly()
    {
        // Arrange
        var columns = Build(CreateDefinition()).ToDictionary(column => column.Field.Name);

        // Act
        var scores = columns["DispatchPart.Scores"];
        var computed = columns["DispatchPart.Computed"];

        // Assert
        // A list of numbers is read as text, which can't be written back as numbers.
        Assert.Null(scores.Write);
        Assert.Null(computed.Write);
        Assert.NotNull(columns["DispatchPart.Tags"].Write);
    }

    [Fact]
    public void Read_MissingNestedObject_ReturnsNull()
    {
        // Arrange
        var columns = Build(CreateDefinition()).ToDictionary(column => column.Field.Name);
        var item = new ContentItem { ContentType = "Dispatch" };
        ((JsonObject)item.Content)["DispatchPart"] = new JsonObject { ["Code"] = "A-12" };

        // Act
        var mode = columns["DispatchPart.Settings.Mode"].Read(item);

        // Assert
        Assert.Null(mode);
    }

    [Theory]
    [InlineData("RouteContainedItems", "Route contained items")]
    [InlineData("ContentItemIds", "Content item ids")]
    [InlineData("URLPath", "URL path")]
    [InlineData("Settings.Mode", "Settings mode")]
    [InlineData("Title", "Title")]
    public void Humanize_PropertyPath_ReturnsWords(string path, string expected)
    {
        // Act
        var words = ContentDataValueDiscovery.Humanize(path);

        // Assert
        Assert.Equal(expected, words);
    }

    private static List<ContentDataColumn> Build(ContentTypeDefinition definition)
    {
        return ContentDataColumns.Build(definition, new ContentDataSourceOptions(), CreateContentOptions(), new PassThroughStringLocalizer<ContentDataColumnsTests>());
    }

    private static ContentOptions CreateContentOptions()
    {
        var services = new ServiceCollection();
        services.AddContentPart<DispatchPart>();
        services.AddContentField<GeoPointField>();
        services.AddContentField<TextField>();
        services.AddContentField<NumericField>();

        return services.BuildServiceProvider().GetRequiredService<IOptions<ContentOptions>>().Value;
    }

    private static ContentTypeDefinition CreateDefinition()
    {
        var dispatchPart = new ContentPartDefinition(
            "DispatchPart",
            [new ContentPartFieldDefinition(new ContentFieldDefinition("GeoPointField"), "Location", [])],
            []);

        var adminPart = new ContentPartDefinition(
            "Dispatch",
            [
                new ContentPartFieldDefinition(new ContentFieldDefinition("TextField"), "Notes", []),
                new ContentPartFieldDefinition(new ContentFieldDefinition("NumericField"), "Cost", []),
                new ContentPartFieldDefinition(new ContentFieldDefinition("UnknownField"), "Unknown", []),
            ],
            []);

        return new ContentTypeDefinition(
            "Dispatch",
            "Dispatch",
            [
                new ContentTypePartDefinition("DispatchPart", dispatchPart, []),
                new ContentTypePartDefinition("Dispatch", adminPart, []),
            ],
            []);
    }

    public sealed class DispatchPart : ContentPart
    {
        public string Code { get; set; }

        public int Rank { get; set; }

        public double Weight { get; set; }

        public bool Urgent { get; set; }

        public DateTime? DueUtc { get; set; }

        public DispatchStatus Status { get; set; }

        public string[] Tags { get; set; } = [];

        public int[] Scores { get; set; } = [];

        public DispatchSettings Settings { get; set; } = new();

        public List<DispatchStop> Stops { get; set; } = [];

        public Dictionary<string, string> Labels { get; set; } = [];

        public JsonObject Raw { get; set; }

        public string Password { get; set; }

        public string ApiKey { get; set; }

        public string RefreshToken { get; set; }

        [JsonIgnore]
        public string Hidden { get; set; }

        public string Computed => Code?.ToUpperInvariant();

        public TextField Summary { get; set; }
    }

    public sealed class DispatchSettings
    {
        public string Mode { get; set; }

        public DispatchLimits Limits { get; set; } = new();
    }

    public sealed class DispatchLimits
    {
        public int Retries { get; set; }
    }

    public sealed class DispatchStop
    {
        public string Address { get; set; }
    }

    public enum DispatchStatus
    {
        Pending,
        Shipped,
    }

    public sealed class GeoPointField : ContentField
    {
        public decimal Latitude { get; set; }

        public decimal Longitude { get; set; }
    }
}
