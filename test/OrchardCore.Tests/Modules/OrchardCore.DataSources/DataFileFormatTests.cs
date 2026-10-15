using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Validation;
using OrchardCore.DataSources;
using OrchardCore.DataSources.Files;
using Sheet = DocumentFormat.OpenXml.Spreadsheet.Sheet;

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

    [Theory]
    [InlineData(DateTimeKind.Utc, "2026-03-15T14:30:05Z")]
    [InlineData(DateTimeKind.Unspecified, "2026-03-15T14:30:05")]
    public void ToText_DateTime_EndsWithZOnlyInUtc(DateTimeKind kind, string expected)
    {
        // Arrange
        var value = new DateTime(2026, 3, 15, 14, 30, 5, kind);

        // Act
        var text = DataFileValues.ToText(value, DataFieldType.DateTime);

        // Assert
        Assert.Equal(expected, text);
    }

    [Fact]
    public async Task CsvWrite_ValuesWithDelimitersQuotesAndNewlines_QuotesThem()
    {
        // Arrange
        var format = new CsvDataFileFormat(new PassThroughStringLocalizer<CsvDataFileFormat>());
        using var stream = new MemoryStream();

        // Act
        var count = await format.WriteAsync(stream, _fields, DataTestHelpers.ToBatches(_fields, _rows), new DataFileOptions(), TestContext.Current.CancellationToken);

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
        await format.WriteAsync(stream, _fields, DataTestHelpers.ToBatches(_fields), new DataFileOptions { UseDisplayNames = true }, TestContext.Current.CancellationToken);

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
        var (fields, rows) = await DataTestHelpers.ReadAllAsync(format.ReadAsync(stream, new DataFileOptions { Delimiter = ";" }, TestContext.Current.CancellationToken));

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
        var (fields, rows) = await DataTestHelpers.ReadAllAsync(format.ReadAsync(stream, new DataFileOptions { HasHeaderRow = false }, TestContext.Current.CancellationToken));

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
        var (fields, _) = await DataTestHelpers.ReadAllAsync(format.ReadAsync(stream, new DataFileOptions(), TestContext.Current.CancellationToken));

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
        var batches = await DataTestHelpers.ToListAsync(format.ReadAsync(stream, new DataFileOptions { BatchSize = 2 }, TestContext.Current.CancellationToken));

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
        var batches = await DataTestHelpers.ToListAsync(format.ReadAsync(stream, new DataFileOptions(), TestContext.Current.CancellationToken));

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
        var count = await format.WriteAsync(stream, _fields, DataTestHelpers.ToBatches(_fields, _rows), new DataFileOptions(), TestContext.Current.CancellationToken);

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
        var (fields, rows) = await DataTestHelpers.ReadAllAsync(format.ReadAsync(stream, new DataFileOptions(), TestContext.Current.CancellationToken));

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
        await format.WriteAsync(stream, _fields, DataTestHelpers.ToBatches(_fields, _rows), new DataFileOptions(), TestContext.Current.CancellationToken);
        var lines = Encoding.UTF8.GetString(stream.ToArray()).Split('\n', StringSplitOptions.RemoveEmptyEntries);
        stream.Position = 0;
        var (fields, rows) = await DataTestHelpers.ReadAllAsync(format.ReadAsync(stream, new DataFileOptions(), TestContext.Current.CancellationToken));

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
        var count = await format.WriteAsync(stream, _fields, DataTestHelpers.ToBatches(_fields, _rows), new DataFileOptions { SheetName = "Orders" }, TestContext.Current.CancellationToken);
        stream.Position = 0;
        var (fields, rows) = await DataTestHelpers.ReadAllAsync(format.ReadAsync(stream, new DataFileOptions { SheetName = "Orders" }, TestContext.Current.CancellationToken));

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
        await format.WriteAsync(stream, _fields, DataTestHelpers.ToBatches(_fields, _rows), new DataFileOptions(), TestContext.Current.CancellationToken);
        stream.Position = 0;

        // Act & Assert
        await Assert.ThrowsAsync<InvalidDataException>(() => DataTestHelpers.ToListAsync(format.ReadAsync(stream, new DataFileOptions { SheetName = "Missing" }, TestContext.Current.CancellationToken)));
    }

    [Fact]
    public async Task ExcelWrite_UseDisplayNames_WritesLabelsInHeader()
    {
        // Arrange
        var format = new ExcelDataFileFormat(new PassThroughStringLocalizer<ExcelDataFileFormat>());
        using var stream = new MemoryStream();

        // Act
        await format.WriteAsync(stream, _fields, DataTestHelpers.ToBatches(_fields, _rows), new DataFileOptions { UseDisplayNames = true }, TestContext.Current.CancellationToken);
        stream.Position = 0;
        var (fields, rows) = await DataTestHelpers.ReadAllAsync(format.ReadAsync(stream, new DataFileOptions(), TestContext.Current.CancellationToken));

        // Assert
        Assert.Equal(["Name", "Quantity", "Unit price", "Active", "Created", "Due"], fields.Select(field => field.Name));
        Assert.Equal(2, rows.Count);
    }

    [Fact]
    public async Task ExcelWrite_NoHeaderRow_WritesOnlyRows()
    {
        // Arrange
        var format = new ExcelDataFileFormat(new PassThroughStringLocalizer<ExcelDataFileFormat>());
        using var stream = new MemoryStream();

        // Act
        await format.WriteAsync(stream, _fields, DataTestHelpers.ToBatches(_fields, _rows), new DataFileOptions { HasHeaderRow = false }, TestContext.Current.CancellationToken);
        stream.Position = 0;
        var (fields, rows) = await DataTestHelpers.ReadAllAsync(format.ReadAsync(stream, new DataFileOptions { HasHeaderRow = false }, TestContext.Current.CancellationToken));

        // Assert
        Assert.Equal(["Column1", "Column2", "Column3", "Column4", "Column5", "Column6"], fields.Select(field => field.Name));
        Assert.Equal(2, rows.Count);
        Assert.Equal("Widget", rows[0][0]);
    }

    [Fact]
    public async Task ExcelWrite_InvalidXmlCharactersAndLongText_StripsAndTruncatesText()
    {
        // Arrange
        var format = new ExcelDataFileFormat(new PassThroughStringLocalizer<ExcelDataFileFormat>());
        DataField[] fields = [new("Text", "Text", DataFieldType.Text)];
        using var stream = new MemoryStream();

        // Act
        await format.WriteAsync(
            stream,
            fields,
            DataTestHelpers.ToBatches(fields, ["a\u0001b\u0000c￾ \r\nd \U0001F600"], [new string('x', 40_000)]),
            new DataFileOptions(),
            TestContext.Current.CancellationToken);
        stream.Position = 0;
        var (_, rows) = await DataTestHelpers.ReadAllAsync(format.ReadAsync(stream, new DataFileOptions(), TestContext.Current.CancellationToken));

        // Assert
        Assert.Equal("abc \r\nd \U0001F600", rows[0][0]);
        Assert.Equal(32_767, ((string)rows[1][0]).Length);
    }

    [Fact]
    public async Task ExcelWrite_NonSeekableStream_WritesReadableWorkbook()
    {
        // Arrange
        var format = new ExcelDataFileFormat(new PassThroughStringLocalizer<ExcelDataFileFormat>());
        using var buffer = new MemoryStream();
        using var stream = new NonSeekableStream(buffer);

        // Act
        var count = await format.WriteAsync(stream, _fields, DataTestHelpers.ToBatches(_fields, _rows), new DataFileOptions(), TestContext.Current.CancellationToken);
        using var copy = new MemoryStream(buffer.ToArray());
        var (_, rows) = await DataTestHelpers.ReadAllAsync(format.ReadAsync(copy, new DataFileOptions(), TestContext.Current.CancellationToken));

        // Assert
        Assert.Equal(2, count);
        Assert.Equal(2, rows.Count);
        Assert.Equal(2.5m, rows[0][2]);
    }

    [Fact]
    public async Task WriteSheetsAsync_SeveralSheets_ReadsEachSheetByName()
    {
        // Arrange
        var format = new ExcelDataFileFormat(new PassThroughStringLocalizer<ExcelDataFileFormat>());
        DataField[] customerFields = [new("Id", "Id", DataFieldType.Integer), new("Email", "Email", DataFieldType.Text)];
        DataField[] totalFields = [new("Day", "Day", DataFieldType.Date), new("Paid", "Paid", DataFieldType.Boolean)];
        using var stream = new MemoryStream();

        // Act
        var counts = await format.WriteSheetsAsync(
            stream,
            [
                new DataSheet("Orders", _fields, DataTestHelpers.ToBatches(_fields, _rows)),
                new DataSheet("Customers", customerFields, DataTestHelpers.ToBatches(customerFields, [1L, "a@example.com"], [2L, "b@example.com"], [3L, null])),
                new DataSheet("Totals", totalFields, DataTestHelpers.ToBatches(totalFields)),
            ],
            new DataFileOptions(),
            TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal([2L, 3L, 0L], counts);

        stream.Position = 0;
        var (orderFields, orders) = await DataTestHelpers.ReadAllAsync(format.ReadAsync(stream, new DataFileOptions { SheetName = "orders" }, TestContext.Current.CancellationToken));
        Assert.Equal(_fields.Select(field => field.Name), orderFields.Select(field => field.Name));
        Assert.Equal(["Widget", 2L, 2.5m, true, new DateTime(2026, 3, 15, 14, 30, 5), new DateTime(2026, 4, 1)], orders[0]);

        stream.Position = 0;
        var (readCustomerFields, customers) = await DataTestHelpers.ReadAllAsync(format.ReadAsync(stream, new DataFileOptions { SheetName = "Customers" }, TestContext.Current.CancellationToken));
        Assert.Equal(["Id", "Email"], readCustomerFields.Select(field => field.Name));
        Assert.Equal([DataFieldType.Integer, DataFieldType.Text], readCustomerFields.Select(field => field.Type));
        Assert.Equal(3, customers.Count);
        Assert.Equal([1L, "a@example.com"], customers[0]);
        Assert.Equal([2L, "b@example.com"], customers[1]);
        Assert.Equal([3L, null], customers[2]);

        stream.Position = 0;
        var (readTotalFields, totals) = await DataTestHelpers.ReadAllAsync(format.ReadAsync(stream, new DataFileOptions { SheetName = "Totals" }, TestContext.Current.CancellationToken));
        Assert.Equal(["Day", "Paid"], readTotalFields.Select(field => field.Name));
        Assert.Empty(totals);

        stream.Position = 0;
        var (firstFields, _) = await DataTestHelpers.ReadAllAsync(format.ReadAsync(stream, new DataFileOptions(), TestContext.Current.CancellationToken));
        Assert.Equal(_fields.Select(field => field.Name), firstFields.Select(field => field.Name));
    }

    [Fact]
    public async Task WriteSheetsAsync_SeveralSheets_EnumeratesEachSheetAfterThePreviousOne()
    {
        // Arrange
        var format = new ExcelDataFileFormat(new PassThroughStringLocalizer<ExcelDataFileFormat>());
        DataField[] fields = [new("N", "N", DataFieldType.Integer)];
        var log = new List<string>();
        using var stream = new MemoryStream();

        // Act
        await format.WriteSheetsAsync(
            stream,
            [
                new DataSheet("A", fields, LoggedBatches(fields, "A", log)),
                new DataSheet("B", fields, LoggedBatches(fields, "B", log)),
            ],
            new DataFileOptions(),
            TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(["A start", "A end", "B start", "B end"], log);
    }

    [Fact]
    public async Task WriteSheetsAsync_InvalidDuplicateOrLongNames_MakesNamesValidAndUnique()
    {
        // Arrange
        var format = new ExcelDataFileFormat(new PassThroughStringLocalizer<ExcelDataFileFormat>());
        DataField[] fields = [new("N", "N", DataFieldType.Integer)];
        string[] names =
        [
            "Sales",
            "sales",
            "SALES",
            "Q1: a/b\\c?d*e[f]",
            null,
            "  ",
            "'Quoted'",
            new string('L', 40),
            new string('L', 40),
        ];
        using var stream = new MemoryStream();

        // Act
        await format.WriteSheetsAsync(stream, names.Select(name => new DataSheet(name, fields, DataTestHelpers.ToBatches(fields, [1L]))).ToArray(), new DataFileOptions(), TestContext.Current.CancellationToken);

        // Assert
        stream.Position = 0;
        using var document = SpreadsheetDocument.Open(stream, false);
        var sheetNames = document.WorkbookPart.Workbook.Sheets.Elements<Sheet>().Select(sheet => sheet.Name.Value).ToArray();
        Assert.Equal(
            [
                "Sales",
                "sales (2)",
                "SALES (3)",
                "Q1_ a_b_c_d_e_f_",
                "Sheet5",
                "Sheet6",
                "Quoted",
                new string('L', 31),
                new string('L', 27) + " (2)",
            ],
            sheetNames);
    }

    [Fact]
    public async Task WriteSheetsAsync_AllFieldTypes_PassesOpenXmlValidation()
    {
        // Arrange
        var format = new ExcelDataFileFormat(new PassThroughStringLocalizer<ExcelDataFileFormat>());
        DataField[] otherFields = [new("Id", "Id", DataFieldType.Integer)];
        using var stream = new MemoryStream();

        // Act
        await format.WriteSheetsAsync(
            stream,
            [
                new DataSheet("Orders", _fields, DataTestHelpers.ToBatches(_fields, _rows)),
                new DataSheet("Other", otherFields, DataTestHelpers.ToBatches(otherFields, [1L], [null])),
            ],
            new DataFileOptions { UseDisplayNames = true },
            TestContext.Current.CancellationToken);

        // Assert
        stream.Position = 0;
        using var document = SpreadsheetDocument.Open(stream, false);
        var errors = new OpenXmlValidator().Validate(document, TestContext.Current.CancellationToken).Select(error => $"{error.Part?.Uri} {error.Path?.XPath}: {error.Description}").ToArray();
        Assert.Empty(errors);
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

    private static async IAsyncEnumerable<DataBatch> LoggedBatches(IReadOnlyList<DataField> fields, string name, List<string> log)
    {
        log.Add($"{name} start");
        await Task.Yield();

        yield return new DataBatch(fields, [[1L]]);

        await Task.Yield();
        log.Add($"{name} end");
    }

    private sealed class NonSeekableStream : Stream
    {
        private readonly Stream _inner;

        public NonSeekableStream(Stream inner)
        {
            _inner = inner;
        }

        public override bool CanRead => false;

        public override bool CanSeek => false;

        public override bool CanWrite => true;

        public override long Length => throw new NotSupportedException();

        public override long Position
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        public override void Flush() => _inner.Flush();

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value) => throw new NotSupportedException();

        public override void Write(byte[] buffer, int offset, int count)
        {
            _inner.Write(buffer, offset, count);
        }
    }
}
