using System.Buffers;
using System.Globalization;
using System.IO.Compression;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.Extensions.Localization;
using OrchardCore.Localization;

namespace OrchardCore.DataSources.Files;

/// <summary>
/// Writes and reads Excel workbooks (.xlsx). Rows are written and read one at a time, so large files never need to fit
/// in memory: each worksheet is streamed into the compressed package as its rows arrive, with its text stored inline.
/// Numbers, booleans, dates and date-times keep their types; when reading, the type of each column is inferred from the
/// values of the first batch. Several data sets can be written to one workbook, each in its own worksheet.
/// </summary>
public sealed class ExcelDataFileFormat : IDataWorkbookFormat
{
    /// <summary>
    /// The technical name of the format.
    /// </summary>
    public const string FormatName = "xlsx";

    private const string IntegerStyle = "1";
    private const string DateStyle = "2";
    private const string DateTimeStyle = "3";
    private const string HeaderStyle = "4";
    private const int MaxCellText = 32_767;
    private const int MaxSheetName = 31;
    private const int MaxWorksheetRows = 1_048_576;

    private const string SpreadsheetNamespace = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
    private const string RelationshipsNamespace = "http://schemas.openxmlformats.org/officeDocument/2006/relationships";
    private const string PackageRelationshipsNamespace = "http://schemas.openxmlformats.org/package/2006/relationships";
    private const string ContentTypesNamespace = "http://schemas.openxmlformats.org/package/2006/content-types";

    private static readonly SearchValues<char> _invalidSheetNameCharacters = SearchValues.Create(":\\/?*[]");

    private readonly IStringLocalizer S;

    public ExcelDataFileFormat(IStringLocalizer<ExcelDataFileFormat> localizer)
    {
        S = localizer;
    }

    public static string Name => FormatName;

    public static LocalizationSource DisplayName { get; } = LocalizationSource.Create<ExcelDataFileFormat>("Excel workbook");

    public static string Extension => ".xlsx";

    public static string ContentType => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    public async Task<long> WriteAsync(
        Stream stream,
        IReadOnlyList<DataField> fields,
        IAsyncEnumerable<DataBatch> batches,
        DataFileOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(stream);
        ArgumentNullException.ThrowIfNull(fields);
        ArgumentNullException.ThrowIfNull(batches);

        var counts = await WriteSheetsAsync(stream, [new DataSheet(options?.SheetName, fields, batches)], options, cancellationToken);

        return counts[0];
    }

    public async Task<IReadOnlyList<long>> WriteSheetsAsync(
        Stream stream,
        IReadOnlyList<DataSheet> sheets,
        DataFileOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(stream);
        ArgumentNullException.ThrowIfNull(sheets);

        if (sheets.Count == 0)
        {
            throw new ArgumentException("A workbook needs at least one sheet.", nameof(sheets));
        }

        if (sheets.Any(sheet => sheet is null))
        {
            throw new ArgumentException("The sheets can't be null.", nameof(sheets));
        }

        options ??= new DataFileOptions();

        var names = SheetNames(sheets);
        var counts = new long[sheets.Count];

        // The package is written in create mode, which streams each part to the output as it is written, instead of
        // keeping the parts in memory until the package is closed.
        var archive = await ZipArchive.CreateAsync(stream, ZipArchiveMode.Create, leaveOpen: true, entryNameEncoding: null, cancellationToken);

        await using (archive)
        {
            await WritePartAsync(archive, "[Content_Types].xml", writer => WriteContentTypesAsync(writer, sheets.Count), cancellationToken);
            await WritePartAsync(archive, "_rels/.rels", WritePackageRelationshipsAsync, cancellationToken);
            await WritePartAsync(archive, "xl/workbook.xml", writer => WriteWorkbookAsync(writer, names), cancellationToken);
            await WritePartAsync(archive, "xl/_rels/workbook.xml.rels", writer => WriteWorkbookRelationshipsAsync(writer, sheets.Count), cancellationToken);
            await WritePartAsync(archive, "xl/styles.xml", WriteStylesheetAsync, cancellationToken);

            for (var index = 0; index < sheets.Count; index++)
            {
                var sheet = sheets[index];
                var name = names[index];
                var count = 0L;

                await WritePartAsync(
                    archive,
                    WorksheetPath(index),
                    async writer => count = await WriteWorksheetAsync(writer, sheet, name, options, cancellationToken),
                    cancellationToken);

                counts[index] = count;
            }
        }

        await stream.FlushAsync(cancellationToken);

        return counts;
    }

    public async IAsyncEnumerable<DataBatch> ReadAsync(
        Stream stream,
        DataFileOptions options,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(stream);

        options ??= new DataFileOptions();

        var batchSize = Math.Max(1, options.BatchSize);

        using var document = SpreadsheetDocument.Open(stream, false);
        var workbookPart = document.WorkbookPart
            ?? throw new InvalidDataException(S["The file is not an Excel workbook."]);

        var sheets = workbookPart.Workbook.Sheets?.Elements<Sheet>().ToList() ?? [];
        var sheet = string.IsNullOrWhiteSpace(options.SheetName)
            ? sheets.FirstOrDefault()
            : sheets.FirstOrDefault(item => string.Equals(item.Name?.Value, options.SheetName.Trim(), StringComparison.OrdinalIgnoreCase));

        if (sheet?.Id?.Value is not { } sheetId)
        {
            throw new InvalidDataException(S["The workbook has no worksheet named '{0}'.", options.SheetName ?? string.Empty]);
        }

        var worksheetPart = (WorksheetPart)workbookPart.GetPartById(sheetId);
        var sharedStrings = workbookPart.SharedStringTablePart?.SharedStringTable?
            .Elements<SharedStringItem>()
            .Select(item => item.InnerText)
            .ToArray() ?? [];
        var styles = ReadStyleKinds(workbookPart.WorkbookStylesPart?.Stylesheet);

        IReadOnlyList<DataField> fields = null;
        List<string> header = null;
        var pending = new List<object[]>(batchSize);
        var yielded = false;

        using var reader = OpenXmlReader.Create(worksheetPart);

        while (reader.Read())
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (reader.ElementType != typeof(Row) || !reader.IsStartElement)
            {
                continue;
            }

            var values = ReadRow((Row)reader.LoadCurrentElement(), sharedStrings, styles);

            if (values.All(value => value is null))
            {
                continue;
            }

            if (header is null && options.HasHeaderRow)
            {
                header = values.Select(value => value is null ? null : DataValues.ToText(value)).ToList();

                continue;
            }

            pending.Add(values);

            if (pending.Count < batchSize)
            {
                continue;
            }

            fields ??= InferFields(header, pending);

            yield return new DataBatch(fields, Coerce(fields, pending));
            yielded = true;
            pending = new List<object[]>(batchSize);
        }

        if (pending.Count > 0 || !yielded)
        {
            fields ??= InferFields(header, pending);

            yield return new DataBatch(fields, Coerce(fields, pending));
        }

        await Task.CompletedTask;
    }

    private static DataField[] InferFields(List<string> header, List<object[]> rows)
    {
        var count = header?.Count ?? (rows.Count == 0 ? 0 : rows.Max(row => row.Length));
        var names = DataFileValues.UniqueNames(Enumerable.Range(0, count).Select(index => header is null ? null : header[index]));

        return names
            .Select((name, index) => new DataField(
                name,
                name,
                DataFileValues.InferType(rows
                    .Where(row => index < row.Length && row[index] is not null)
                    .Select(row => TypeOf(row[index])))))
            .ToArray();
    }

    private static List<object[]> Coerce(IReadOnlyList<DataField> fields, List<object[]> rows)
    {
        var result = new List<object[]>(rows.Count);

        foreach (var values in rows)
        {
            var row = new object[fields.Count];

            for (var index = 0; index < row.Length && index < values.Length; index++)
            {
                row[index] = values[index] switch
                {
                    null => null,
                    ExcelDate date => DataValues.Coerce(date.Value, fields[index].Type),
                    var value => DataValues.Coerce(value, fields[index].Type),
                };
            }

            result.Add(row);
        }

        return result;
    }

    private static DataFieldType TypeOf(object value)
    {
        return value switch
        {
            ExcelDate date => date.HasTime ? DataFieldType.DateTime : DataFieldType.Date,
            long => DataFieldType.Integer,
            decimal => DataFieldType.Decimal,
            bool => DataFieldType.Boolean,
            _ => DataFieldType.Text,
        };
    }

    private static object[] ReadRow(Row row, string[] sharedStrings, IReadOnlyList<StyleKind> styles)
    {
        var values = new List<object>();
        var position = 0;

        foreach (var cell in row.Elements<Cell>())
        {
            var column = ColumnIndex(cell.CellReference?.Value) ?? position;

            while (values.Count < column)
            {
                values.Add(null);
            }

            values.Add(ReadCell(cell, sharedStrings, styles));
            position = column + 1;
        }

        return [.. values];
    }

    private static object ReadCell(Cell cell, string[] sharedStrings, IReadOnlyList<StyleKind> styles)
    {
        var text = cell.CellValue?.Text;
        var type = cell.DataType?.Value;

        if (type == CellValues.InlineString)
        {
            return EmptyToNull(cell.InlineString?.InnerText);
        }

        if (type == CellValues.SharedString)
        {
            return int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var index) && index >= 0 && index < sharedStrings.Length
                ? EmptyToNull(sharedStrings[index])
                : null;
        }

        if (type == CellValues.String)
        {
            return EmptyToNull(text);
        }

        if (type == CellValues.Boolean)
        {
            return text == "1" || string.Equals(text, "true", StringComparison.OrdinalIgnoreCase);
        }

        if (type == CellValues.Error || string.IsNullOrEmpty(text))
        {
            return null;
        }

        if (type == CellValues.Date)
        {
            return DateTime.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed)
                ? new ExcelDate(parsed, parsed.TimeOfDay != TimeSpan.Zero)
                : null;
        }

        var style = cell.StyleIndex?.Value is { } styleIndex && styleIndex < styles.Count ? styles[(int)styleIndex] : StyleKind.General;

        if (style is StyleKind.Date or StyleKind.DateTime)
        {
            if (double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var serial))
            {
                try
                {
                    var date = DateTime.FromOADate(serial);

                    return new ExcelDate(date, style == StyleKind.DateTime);
                }
                catch (ArgumentException)
                {
                    return null;
                }
            }

            return null;
        }

        if (!decimal.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var number))
        {
            return text;
        }

        return style == StyleKind.Integer && decimal.Truncate(number) == number && number >= long.MinValue && number <= long.MaxValue
            ? (object)(long)number
            : number;
    }

    private static int? ColumnIndex(string reference)
    {
        if (string.IsNullOrEmpty(reference))
        {
            return null;
        }

        var index = 0;
        var letters = 0;

        foreach (var character in reference)
        {
            if (!char.IsLetter(character))
            {
                break;
            }

            index = (index * 26) + (char.ToUpperInvariant(character) - 'A' + 1);
            letters++;
        }

        return letters == 0 ? null : index - 1;
    }

    private static StyleKind[] ReadStyleKinds(Stylesheet stylesheet)
    {
        if (stylesheet?.CellFormats is null)
        {
            return [];
        }

        var customFormats = stylesheet.NumberingFormats?
            .Elements<NumberingFormat>()
            .Where(format => format.NumberFormatId?.Value is not null)
            .ToDictionary(format => format.NumberFormatId.Value, format => format.FormatCode?.Value ?? string.Empty)
            ?? [];

        return stylesheet.CellFormats
            .Elements<CellFormat>()
            .Select(format => KindOf(format.NumberFormatId?.Value ?? 0, customFormats))
            .ToArray();
    }

    private static StyleKind KindOf(uint numberFormatId, Dictionary<uint, string> customFormats)
    {
        switch (numberFormatId)
        {
            case 1 or 3:
                return StyleKind.Integer;
            case >= 14 and <= 17:
                return StyleKind.Date;
            case 18 or 19 or 20 or 21 or 22 or 45 or 46 or 47:
                return StyleKind.DateTime;
        }

        if (!customFormats.TryGetValue(numberFormatId, out var code) || string.IsNullOrEmpty(code))
        {
            return StyleKind.General;
        }

        // Ignore quoted literals and bracketed sections such as colors and locales.
        var stripped = new System.Text.StringBuilder();
        var inQuotes = false;
        var inBrackets = false;

        foreach (var character in code)
        {
            if (character == '"')
            {
                inQuotes = !inQuotes;
            }
            else if (!inQuotes && character == '[')
            {
                inBrackets = true;
            }
            else if (!inQuotes && character == ']')
            {
                inBrackets = false;
            }
            else if (!inQuotes && !inBrackets)
            {
                stripped.Append(char.ToLowerInvariant(character));
            }
        }

        var format = stripped.ToString();
        var hasDate = format.Contains('y') || format.Contains('d') || (format.Contains('m') && !format.Contains('h') && !format.Contains('s'));
        var hasTime = format.Contains('h') || format.Contains('s');

        if (hasDate || hasTime)
        {
            return hasTime ? StyleKind.DateTime : StyleKind.Date;
        }

        return format == "0" || format == "#,##0" ? StyleKind.Integer : StyleKind.General;
    }

    private static async Task WritePartAsync(ZipArchive archive, string path, Func<XmlWriter, Task> write, CancellationToken cancellationToken)
    {
        var entry = archive.CreateEntry(path, CompressionLevel.Optimal);

        await using var entryStream = await entry.OpenAsync(cancellationToken);
        await using var writer = XmlWriter.Create(entryStream, new XmlWriterSettings
        {
            Async = true,
            Encoding = new UTF8Encoding(false),
            CloseOutput = false,

            // Keeps carriage returns of cell text, which XML readers would otherwise normalize to line feeds.
            NewLineHandling = NewLineHandling.Entitize,
        });

        await writer.WriteStartDocumentAsync(true);
        await write(writer);
        await writer.WriteEndDocumentAsync();
        await writer.FlushAsync();
    }

    private static string WorksheetPath(int index)
    {
        return $"xl/worksheets/sheet{(index + 1).ToString(CultureInfo.InvariantCulture)}.xml";
    }

    private static string RelationshipId(int index)
    {
        return $"rId{(index + 1).ToString(CultureInfo.InvariantCulture)}";
    }

    private static async Task WriteContentTypesAsync(XmlWriter writer, int sheetCount)
    {
        await writer.WriteStartElementAsync(null, "Types", ContentTypesNamespace);
        await WriteContentTypeAsync(writer, "Default", "Extension", "rels", "application/vnd.openxmlformats-package.relationships+xml");
        await WriteContentTypeAsync(writer, "Default", "Extension", "xml", "application/xml");
        await WriteContentTypeAsync(writer, "Override", "PartName", "/xl/workbook.xml", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml");
        await WriteContentTypeAsync(writer, "Override", "PartName", "/xl/styles.xml", "application/vnd.openxmlformats-officedocument.spreadsheetml.styles+xml");

        for (var index = 0; index < sheetCount; index++)
        {
            await WriteContentTypeAsync(writer, "Override", "PartName", "/" + WorksheetPath(index), "application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml");
        }

        await writer.WriteEndElementAsync();
    }

    private static async Task WriteContentTypeAsync(XmlWriter writer, string element, string attribute, string value, string contentType)
    {
        await writer.WriteStartElementAsync(null, element, ContentTypesNamespace);
        await writer.WriteAttributeStringAsync(null, attribute, null, value);
        await writer.WriteAttributeStringAsync(null, "ContentType", null, contentType);
        await writer.WriteEndElementAsync();
    }

    private static async Task WritePackageRelationshipsAsync(XmlWriter writer)
    {
        await writer.WriteStartElementAsync(null, "Relationships", PackageRelationshipsNamespace);
        await WriteRelationshipAsync(writer, "rId1", "officeDocument", "xl/workbook.xml");
        await writer.WriteEndElementAsync();
    }

    private static async Task WriteWorkbookRelationshipsAsync(XmlWriter writer, int sheetCount)
    {
        await writer.WriteStartElementAsync(null, "Relationships", PackageRelationshipsNamespace);

        for (var index = 0; index < sheetCount; index++)
        {
            await WriteRelationshipAsync(writer, RelationshipId(index), "worksheet", $"worksheets/sheet{(index + 1).ToString(CultureInfo.InvariantCulture)}.xml");
        }

        await WriteRelationshipAsync(writer, RelationshipId(sheetCount), "styles", "styles.xml");
        await writer.WriteEndElementAsync();
    }

    private static async Task WriteRelationshipAsync(XmlWriter writer, string id, string type, string target)
    {
        await writer.WriteStartElementAsync(null, "Relationship", PackageRelationshipsNamespace);
        await writer.WriteAttributeStringAsync(null, "Id", null, id);
        await writer.WriteAttributeStringAsync(null, "Type", null, RelationshipsNamespace + "/" + type);
        await writer.WriteAttributeStringAsync(null, "Target", null, target);
        await writer.WriteEndElementAsync();
    }

    private static async Task WriteWorkbookAsync(XmlWriter writer, string[] names)
    {
        await writer.WriteStartElementAsync(null, "workbook", SpreadsheetNamespace);
        await writer.WriteAttributeStringAsync("xmlns", "r", null, RelationshipsNamespace);
        await writer.WriteStartElementAsync(null, "sheets", SpreadsheetNamespace);

        for (var index = 0; index < names.Length; index++)
        {
            await writer.WriteStartElementAsync(null, "sheet", SpreadsheetNamespace);
            await writer.WriteAttributeStringAsync(null, "name", null, names[index]);
            await writer.WriteAttributeStringAsync(null, "sheetId", null, (index + 1).ToString(CultureInfo.InvariantCulture));
            await writer.WriteAttributeStringAsync("r", "id", RelationshipsNamespace, RelationshipId(index));
            await writer.WriteEndElementAsync();
        }

        await writer.WriteEndElementAsync();
        await writer.WriteEndElementAsync();
    }

    // The cell formats, by index: general, integer, date, date-time and the bold header.
    private static async Task WriteStylesheetAsync(XmlWriter writer)
    {
        await writer.WriteStartElementAsync(null, "styleSheet", SpreadsheetNamespace);

        await writer.WriteStartElementAsync(null, "fonts", SpreadsheetNamespace);
        await writer.WriteAttributeStringAsync(null, "count", null, "2");
        await WriteFontAsync(writer, bold: false);
        await WriteFontAsync(writer, bold: true);
        await writer.WriteEndElementAsync();

        await writer.WriteStartElementAsync(null, "fills", SpreadsheetNamespace);
        await writer.WriteAttributeStringAsync(null, "count", null, "2");

        foreach (var pattern in (string[])["none", "gray125"])
        {
            await writer.WriteStartElementAsync(null, "fill", SpreadsheetNamespace);
            await writer.WriteStartElementAsync(null, "patternFill", SpreadsheetNamespace);
            await writer.WriteAttributeStringAsync(null, "patternType", null, pattern);
            await writer.WriteEndElementAsync();
            await writer.WriteEndElementAsync();
        }

        await writer.WriteEndElementAsync();

        await writer.WriteStartElementAsync(null, "borders", SpreadsheetNamespace);
        await writer.WriteAttributeStringAsync(null, "count", null, "1");
        await writer.WriteStartElementAsync(null, "border", SpreadsheetNamespace);

        foreach (var side in (string[])["left", "right", "top", "bottom", "diagonal"])
        {
            await writer.WriteStartElementAsync(null, side, SpreadsheetNamespace);
            await writer.WriteEndElementAsync();
        }

        await writer.WriteEndElementAsync();
        await writer.WriteEndElementAsync();

        await writer.WriteStartElementAsync(null, "cellStyleXfs", SpreadsheetNamespace);
        await writer.WriteAttributeStringAsync(null, "count", null, "1");
        await WriteCellFormatAsync(writer, numberFormatId: "0", fontId: "0", isStyleFormat: true);
        await writer.WriteEndElementAsync();

        await writer.WriteStartElementAsync(null, "cellXfs", SpreadsheetNamespace);
        await writer.WriteAttributeStringAsync(null, "count", null, "5");
        await WriteCellFormatAsync(writer, numberFormatId: "0", fontId: "0");
        await WriteCellFormatAsync(writer, numberFormatId: "1", fontId: "0");
        await WriteCellFormatAsync(writer, numberFormatId: "14", fontId: "0");
        await WriteCellFormatAsync(writer, numberFormatId: "22", fontId: "0");
        await WriteCellFormatAsync(writer, numberFormatId: "0", fontId: "1");
        await writer.WriteEndElementAsync();

        await writer.WriteStartElementAsync(null, "cellStyles", SpreadsheetNamespace);
        await writer.WriteAttributeStringAsync(null, "count", null, "1");
        await writer.WriteStartElementAsync(null, "cellStyle", SpreadsheetNamespace);
        await writer.WriteAttributeStringAsync(null, "name", null, "Normal");
        await writer.WriteAttributeStringAsync(null, "xfId", null, "0");
        await writer.WriteAttributeStringAsync(null, "builtinId", null, "0");
        await writer.WriteEndElementAsync();
        await writer.WriteEndElementAsync();

        await writer.WriteEndElementAsync();
    }

    private static async Task WriteFontAsync(XmlWriter writer, bool bold)
    {
        await writer.WriteStartElementAsync(null, "font", SpreadsheetNamespace);

        if (bold)
        {
            await writer.WriteStartElementAsync(null, "b", SpreadsheetNamespace);
            await writer.WriteEndElementAsync();
        }

        await writer.WriteStartElementAsync(null, "sz", SpreadsheetNamespace);
        await writer.WriteAttributeStringAsync(null, "val", null, "11");
        await writer.WriteEndElementAsync();
        await writer.WriteStartElementAsync(null, "name", SpreadsheetNamespace);
        await writer.WriteAttributeStringAsync(null, "val", null, "Calibri");
        await writer.WriteEndElementAsync();
        await writer.WriteStartElementAsync(null, "family", SpreadsheetNamespace);
        await writer.WriteAttributeStringAsync(null, "val", null, "2");
        await writer.WriteEndElementAsync();
        await writer.WriteEndElementAsync();
    }

    private static async Task WriteCellFormatAsync(XmlWriter writer, string numberFormatId, string fontId, bool isStyleFormat = false)
    {
        await writer.WriteStartElementAsync(null, "xf", SpreadsheetNamespace);
        await writer.WriteAttributeStringAsync(null, "numFmtId", null, numberFormatId);
        await writer.WriteAttributeStringAsync(null, "fontId", null, fontId);
        await writer.WriteAttributeStringAsync(null, "fillId", null, "0");
        await writer.WriteAttributeStringAsync(null, "borderId", null, "0");

        if (!isStyleFormat)
        {
            await writer.WriteAttributeStringAsync(null, "xfId", null, "0");

            if (numberFormatId != "0")
            {
                await writer.WriteAttributeStringAsync(null, "applyNumberFormat", null, "1");
            }

            if (fontId != "0")
            {
                await writer.WriteAttributeStringAsync(null, "applyFont", null, "1");
            }
        }

        await writer.WriteEndElementAsync();
    }

    // Cells and rows are written without references: their positions place them. Incrementing references would make
    // the compressed worksheet about twice as large.
    private async Task<long> WriteWorksheetAsync(XmlWriter writer, DataSheet sheet, string sheetName, DataFileOptions options, CancellationToken cancellationToken)
    {
        // Excel doesn't open the rows of a worksheet past this one.
        var maxRows = MaxWorksheetRows - (options.HasHeaderRow ? 1 : 0);
        var fields = sheet.Fields;
        var types = new DataFieldType[fields.Count];

        for (var index = 0; index < fields.Count; index++)
        {
            types[index] = fields[index].Type;
        }

        await writer.WriteStartElementAsync(null, "worksheet", SpreadsheetNamespace);
        await writer.WriteStartElementAsync(null, "sheetData", SpreadsheetNamespace);

        if (options.HasHeaderRow)
        {
            await writer.WriteStartElementAsync(null, "row", SpreadsheetNamespace);

            foreach (var field in fields)
            {
                var name = options.UseDisplayNames && !string.IsNullOrEmpty(field.DisplayName) ? field.DisplayName : field.Name;

                await WriteTextCellAsync(writer, name, HeaderStyle);
            }

            await writer.WriteEndElementAsync();
        }

        var count = 0L;

        await foreach (var batch in sheet.Batches.WithCancellation(cancellationToken))
        {
            foreach (var row in batch.Rows)
            {
                if (count == maxRows)
                {
                    throw new InvalidOperationException(S["The worksheet '{0}' can hold {1} rows at most. Write the rows to a CSV file, or split them into several worksheets.", sheetName, maxRows]);
                }

                await writer.WriteStartElementAsync(null, "row", SpreadsheetNamespace);

                for (var index = 0; index < types.Length; index++)
                {
                    await WriteCellAsync(writer, index < row.Length ? row[index] : null, types[index]);
                }

                await writer.WriteEndElementAsync();
                count++;
            }
        }

        await writer.WriteEndElementAsync();
        await writer.WriteEndElementAsync();

        return count;
    }

    private static Task WriteCellAsync(XmlWriter writer, object value, DataFieldType type)
    {
        if (value is null)
        {
            return WriteEmptyCellAsync(writer, null);
        }

        switch (type)
        {
            case DataFieldType.Integer when DataValues.Coerce(value, DataFieldType.Integer) is long number:
                return WriteValueCellAsync(writer, IntegerStyle, null, number.ToString(CultureInfo.InvariantCulture));

            case DataFieldType.Decimal when DataValues.Coerce(value, DataFieldType.Decimal) is decimal number:
                return WriteValueCellAsync(writer, null, null, number.ToString(CultureInfo.InvariantCulture));

            case DataFieldType.Boolean when DataValues.Coerce(value, DataFieldType.Boolean) is bool flag:
                return WriteValueCellAsync(writer, null, "b", flag ? "1" : "0");

            case DataFieldType.Date when DataValues.Coerce(value, DataFieldType.Date) is DateTime date:
                return WriteValueCellAsync(writer, DateStyle, null, date.ToOADate().ToString("R", CultureInfo.InvariantCulture));

            case DataFieldType.DateTime when DataValues.Coerce(value, DataFieldType.DateTime) is DateTime dateTime:
                return WriteValueCellAsync(writer, DateTimeStyle, null, dateTime.ToOADate().ToString("R", CultureInfo.InvariantCulture));

            default:
                return WriteTextCellAsync(writer, DataFileValues.ToText(value, type), null);
        }
    }

    private static async Task WriteValueCellAsync(XmlWriter writer, string style, string cellType, string value)
    {
        await WriteStartCellAsync(writer, style, cellType);
        await writer.WriteStartElementAsync(null, "v", SpreadsheetNamespace);
        await writer.WriteStringAsync(value);
        await writer.WriteEndElementAsync();
        await writer.WriteEndElementAsync();
    }

    private static async Task WriteTextCellAsync(XmlWriter writer, string text, string style)
    {
        text = string.IsNullOrEmpty(text) ? null : CellText(text);

        if (string.IsNullOrEmpty(text))
        {
            await WriteEmptyCellAsync(writer, style);

            return;
        }

        await WriteStartCellAsync(writer, style, "inlineStr");
        await writer.WriteStartElementAsync(null, "is", SpreadsheetNamespace);
        await writer.WriteStartElementAsync(null, "t", SpreadsheetNamespace);

        if (char.IsWhiteSpace(text[0]) || char.IsWhiteSpace(text[^1]))
        {
            await writer.WriteAttributeStringAsync("xml", "space", null, "preserve");
        }

        await writer.WriteStringAsync(text);
        await writer.WriteEndElementAsync();
        await writer.WriteEndElementAsync();
        await writer.WriteEndElementAsync();
    }

    // An empty cell keeps the position of the next cells of its row.
    private static async Task WriteEmptyCellAsync(XmlWriter writer, string style)
    {
        await WriteStartCellAsync(writer, style, null);
        await writer.WriteEndElementAsync();
    }

    private static async Task WriteStartCellAsync(XmlWriter writer, string style, string cellType)
    {
        await writer.WriteStartElementAsync(null, "c", SpreadsheetNamespace);

        if (style is not null)
        {
            await writer.WriteAttributeStringAsync(null, "s", null, style);
        }

        if (cellType is not null)
        {
            await writer.WriteAttributeStringAsync(null, "t", null, cellType);
        }
    }

    // Removes the characters XML can't hold, and keeps the text within the length Excel allows in a cell.
    private static string CellText(string text)
    {
        return Truncate(RemoveInvalidXmlCharacters(text), MaxCellText);
    }

    private static string RemoveInvalidXmlCharacters(string text)
    {
        StringBuilder builder = null;

        for (var index = 0; index < text.Length; index++)
        {
            var character = text[index];

            if (XmlConvert.IsXmlChar(character))
            {
                builder?.Append(character);

                continue;
            }

            if (index + 1 < text.Length && XmlConvert.IsXmlSurrogatePair(text[index + 1], character))
            {
                builder?.Append(character).Append(text[index + 1]);
                index++;

                continue;
            }

            builder ??= new StringBuilder(text.Length).Append(text, 0, index);
        }

        return builder?.ToString() ?? text;
    }

    // Keeps whole surrogate pairs when cutting the text.
    private static string Truncate(string value, int length)
    {
        if (value.Length <= length)
        {
            return value;
        }

        if (char.IsHighSurrogate(value[length - 1]))
        {
            length--;
        }

        return value[..length];
    }

    // Excel requires worksheet names of 1 to 31 characters, without : \ / ? * [ ] or a leading or trailing apostrophe,
    // and unique within a workbook ignoring case.
    private static string[] SheetNames(IReadOnlyList<DataSheet> sheets)
    {
        var names = new string[sheets.Count];
        var used = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        for (var index = 0; index < sheets.Count; index++)
        {
            var name = CleanSheetName(sheets[index].Name);

            if (string.IsNullOrEmpty(name))
            {
                name = $"Sheet{(index + 1).ToString(CultureInfo.InvariantCulture)}";
            }

            var candidate = name;
            var suffix = 2;

            while (!used.Add(candidate))
            {
                var tail = $" ({suffix.ToString(CultureInfo.InvariantCulture)})";
                candidate = TrimSheetName(Truncate(name, MaxSheetName - tail.Length)) + tail;
                suffix++;
            }

            names[index] = candidate;
        }

        return names;
    }

    private static string CleanSheetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return null;
        }

        name = RemoveInvalidXmlCharacters(name);

        if (name.AsSpan().ContainsAny(_invalidSheetNameCharacters))
        {
            var characters = name.ToCharArray();

            for (var index = 0; index < characters.Length; index++)
            {
                if (_invalidSheetNameCharacters.Contains(characters[index]))
                {
                    characters[index] = '_';
                }
            }

            name = new string(characters);
        }

        return TrimSheetName(Truncate(TrimSheetName(name), MaxSheetName));
    }

    private static string TrimSheetName(string name) => name.Trim().Trim('\'').Trim();

    private static string EmptyToNull(string value) => string.IsNullOrEmpty(value) ? null : value;

    private enum StyleKind
    {
        General,
        Integer,
        Date,
        DateTime,
    }

    private sealed record ExcelDate(DateTime Value, bool HasTime);
}
