using OrchardCore.DataSources;
using OrchardCore.DataSources.Files;

namespace OrchardCore.Tests.Modules.OrchardCore.DataSources;

public sealed class DataFileFormatTests
{
    private static readonly DataField[] _fields =
    [
        new("Name", "Name", DataFieldType.Text),
        new("Quantity", "Quantity", DataFieldType.Integer),
        new("Price", "Unit price", DataFieldType.Decimal),
        new("Active", "Active", DataFieldType.Boolean),
        new("Created", "Created", DataFieldType.DateTime),
        new("Due", "Due", DataFieldType.Date),
    ];

    private static readonly object[][] _rows =
    [
        ["Widget", 2L, 2.5m, true, new DateTime(2026, 3, 15, 14, 30, 5, DateTimeKind.Utc), new DateTime(2026, 4, 1)],
        ["Gadget, \"large\"\nsecond line", null, 10m, false, null, null],
    ];

    [Fact]
    public async Task CsvWrite_ValuesWithDelimitersQuotesAndNewlines_QuotesThem()
    {
        // Arrange
        var format = new CsvDataFileFormat(new PassThroughStringLocalizer<CsvDataFileFormat>());
        using var stream = new MemoryStream();

        // Act
        var count = await format.WriteAsync(stream, _fields, DataTestHelpers.ToBatches(_fields, _rows), new DataFileOptions());

        // Assert
        var text = Encoding.UTF8.GetString(stream.ToArray()).TrimStart('﻿');
        Assert.Equal(2, count);
        Assert.Equal(
            "Name,Quantity,Price,Active,Created,Due\r\n" +
            "Widget,2,2.5,true,2026-03-15T14:30:05Z,2026-04-01\r\n" +
            "\"Gadget, \"\"large\"\"\nsecond line\",,10,false,,\r\n",
            text);
    }

    [Fact]
    public async Task CsvWrite_UseDisplayNames_WritesLabelsInHeader()
    {
        // Arrange
        var format = new CsvDataFileFormat(new PassThroughStringLocalizer<CsvDataFileFormat>());
        using var stream = new MemoryStream();

        // Act
        await format.WriteAsync(stream, _fields, DataTestHelpers.ToBatches(_fields), new DataFileOptions { UseDisplayNames = true });

        // Assert
        var text = Encoding.UTF8.GetString(stream.ToArray()).TrimStart('﻿');
        Assert.Equal("Name,Quantity,Unit price,Active,Created,Due\r\n", text);
    }

    [Fact]
    public async Task CsvRead_QuotedValues_ReturnsTextFieldsAndRows()
    {
        // Arrange
        var format = new CsvDataFileFormat(new PassThroughStringLocalizer<CsvDataFileFormat>());
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("﻿Name;Note\n\"a;b\";\"say \"\"hi\"\"\nthere\"\r\nc;\n"));

        // Act
        var (fields, rows) = await DataTestHelpers.ReadAllAsync(format.ReadAsync(stream, new DataFileOptions { Delimiter = ";" }));

        // Assert
        Assert.Equal(["Name", "Note"], fields.Select(field => field.Name));
        Assert.All(fields, field => Assert.Equal(DataFieldType.Text, field.Type));
        Assert.Equal(2, rows.Count);
        Assert.Equal(["a;b", "say \"hi\"\nthere"], rows[0]);
        Assert.Equal(["c", null], rows[1]);
    }

    [Fact]
    public async Task CsvRead_NoHeaderRow_NamesColumnsByPosition()
    {
        // Arrange
        var format = new CsvDataFileFormat(new PassThroughStringLocalizer<CsvDataFileFormat>());
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("1,2\n3,4,5\n"));

        // Act
        var (fields, rows) = await DataTestHelpers.ReadAllAsync(format.ReadAsync(stream, new DataFileOptions { HasHeaderRow = false }));

        // Assert
        Assert.Equal(["Column1", "Column2"], fields.Select(field => field.Name));
        Assert.Equal(["1", "2"], rows[0]);
        Assert.Equal(["3", "4"], rows[1]);
    }

    [Fact]
    public async Task CsvRead_DuplicateOrEmptyHeaders_MakesNamesUnique()
    {
        // Arrange
        var format = new CsvDataFileFormat(new PassThroughStringLocalizer<CsvDataFileFormat>());
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("Name,Name,\nx,y,z\n"));

        // Act
        var (fields, _) = await DataTestHelpers.ReadAllAsync(format.ReadAsync(stream, new DataFileOptions()));

        // Assert
        Assert.Equal(["Name", "Name2", "Column3"], fields.Select(field => field.Name));
    }

    [Fact]
    public async Task CsvRead_BatchSize_SplitsRowsIntoBatches()
    {
        // Arrange
        var format = new CsvDataFileFormat(new PassThroughStringLocalizer<CsvDataFileFormat>());
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("N\n1\n2\n3\n4\n5\n"));

        // Act
        var batches = await DataTestHelpers.ToListAsync(format.ReadAsync(stream, new DataFileOptions { BatchSize = 2 }));

        // Assert
        Assert.Equal([2, 2, 1], batches.Select(batch => batch.Count));
    }

    [Fact]
    public async Task CsvRead_HeaderOnly_YieldsOneEmptyBatchWithFields()
    {
        // Arrange
        var format = new CsvDataFileFormat(new PassThroughStringLocalizer<CsvDataFileFormat>());
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("A,B\n"));

        // Act
        var batches = await DataTestHelpers.ToListAsync(format.ReadAsync(stream, new DataFileOptions()));

        // Assert
        var batch = Assert.Single(batches);
        Assert.Equal(0, batch.Count);
        Assert.Equal(["A", "B"], batch.Fields.Select(field => field.Name));
    }

    [Fact]
    public async Task JsonWrite_Rows_WritesArrayOfTypedObjects()
    {
        // Arrange
        var format = new JsonDataFileFormat(new PassThroughStringLocalizer<JsonDataFileFormat>());
        using var stream = new MemoryStream();

        // Act
        var count = await format.WriteAsync(stream, _fields, DataTestHelpers.ToBatches(_fields, _rows), new DataFileOptions());

        // Assert
        var text = Encoding.UTF8.GetString(stream.ToArray());
        Assert.Equal(2, count);
        Assert.Equal(
            "[{\"Name\":\"Widget\",\"Quantity\":2,\"Price\":2.5,\"Active\":true,\"Created\":\"2026-03-15T14:30:05Z\",\"Due\":\"2026-04-01\"}," +
            "{\"Name\":\"Gadget, \\u0022large\\u0022\\nsecond line\",\"Quantity\":null,\"Price\":10,\"Active\":false,\"Created\":null,\"Due\":null}]",
            text);
    }

    [Fact]
    public async Task JsonRead_Objects_InfersFieldTypesFromValues()
    {
        // Arrange
        var format = new JsonDataFileFormat(new PassThroughStringLocalizer<JsonDataFileFormat>());
        var json = "[{\"Id\":1,\"Name\":\"a\",\"Price\":1.5,\"On\":true,\"Tags\":[\"x\"]},{\"Id\":2,\"Name\":\"b\",\"Extra\":\"later\"}]";
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));

        // Act
        var (fields, rows) = await DataTestHelpers.ReadAllAsync(format.ReadAsync(stream, new DataFileOptions()));

        // Assert
        Assert.Equal(["Id", "Name", "Price", "On", "Tags", "Extra"], fields.Select(field => field.Name));
        Assert.Equal(
            [DataFieldType.Integer, DataFieldType.Text, DataFieldType.Decimal, DataFieldType.Boolean, DataFieldType.Text, DataFieldType.Text],
            fields.Select(field => field.Type));
        Assert.Equal([1L, "a", 1.5m, true, "[\"x\"]", null], rows[0]);
        Assert.Equal([2L, "b", null, null, null, "later"], rows[1]);
    }

    [Fact]
    public async Task JsonLinesRoundTrip_Rows_PreservesValues()
    {
        // Arrange
        var format = new JsonLinesDataFileFormat(new PassThroughStringLocalizer<JsonLinesDataFileFormat>());
        using var stream = new MemoryStream();

        // Act
        await format.WriteAsync(stream, _fields, DataTestHelpers.ToBatches(_fields, _rows), new DataFileOptions());
        var lines = Encoding.UTF8.GetString(stream.ToArray()).Split('\n', StringSplitOptions.RemoveEmptyEntries);
        stream.Position = 0;
        var (fields, rows) = await DataTestHelpers.ReadAllAsync(format.ReadAsync(stream, new DataFileOptions()));

        // Assert
        Assert.Equal(2, lines.Length);
        Assert.Equal(_fields.Select(field => field.Name), fields.Select(field => field.Name));
        Assert.Equal("Widget", rows[0][0]);
        Assert.Equal(2L, rows[0][1]);
        Assert.Equal(2.5m, rows[0][2]);
        Assert.Equal("Gadget, \"large\"\nsecond line", rows[1][0]);
    }

    [Fact]
    public async Task ExcelRoundTrip_TypedRows_PreservesValuesAndTypes()
    {
        // Arrange
        var format = new ExcelDataFileFormat(new PassThroughStringLocalizer<ExcelDataFileFormat>());
        using var stream = new MemoryStream();

        // Act
        var count = await format.WriteAsync(stream, _fields, DataTestHelpers.ToBatches(_fields, _rows), new DataFileOptions { SheetName = "Orders" });
        stream.Position = 0;
        var (fields, rows) = await DataTestHelpers.ReadAllAsync(format.ReadAsync(stream, new DataFileOptions { SheetName = "Orders" }));

        // Assert
        Assert.Equal(2, count);
        Assert.Equal(_fields.Select(field => field.Name), fields.Select(field => field.Name));
        Assert.Equal(
            [DataFieldType.Text, DataFieldType.Integer, DataFieldType.Decimal, DataFieldType.Boolean, DataFieldType.DateTime, DataFieldType.Date],
            fields.Select(field => field.Type));
        Assert.Equal(["Widget", 2L, 2.5m, true, new DateTime(2026, 3, 15, 14, 30, 5), new DateTime(2026, 4, 1)], rows[0]);
        Assert.Equal(["Gadget, \"large\"\nsecond line", null, 10m, false, null, null], rows[1]);
    }

    [Fact]
    public async Task ExcelRead_UnknownSheet_Throws()
    {
        // Arrange
        var format = new ExcelDataFileFormat(new PassThroughStringLocalizer<ExcelDataFileFormat>());
        using var stream = new MemoryStream();
        await format.WriteAsync(stream, _fields, DataTestHelpers.ToBatches(_fields, _rows), new DataFileOptions());
        stream.Position = 0;

        // Act & Assert
        await Assert.ThrowsAsync<InvalidDataException>(() => DataTestHelpers.ToListAsync(format.ReadAsync(stream, new DataFileOptions { SheetName = "Missing" })));
    }

    [Fact]
    public void GetFormatByFileName_KnownExtension_ReturnsFormatIgnoringCase()
    {
        // Arrange
        var manager = new DataFileFormatManager([new CsvDataFileFormat(new PassThroughStringLocalizer<CsvDataFileFormat>()), new JsonDataFileFormat(new PassThroughStringLocalizer<JsonDataFileFormat>()), new ExcelDataFileFormat(new PassThroughStringLocalizer<ExcelDataFileFormat>())]);

        // Act
        var format = manager.GetFormatByFileName("exports/ORDERS.XLSX");

        // Assert
        Assert.Equal("xlsx", format.Name);
        Assert.Null(manager.GetFormatByFileName("notes.txt"));
        Assert.Equal("csv", manager.GetFormat("CSV").Name);
    }
}
