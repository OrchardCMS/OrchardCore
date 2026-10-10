using System.Globalization;
using System.Runtime.CompilerServices;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.Extensions.Localization;

namespace OrchardCore.DataSources.Files;

/// <summary>
/// Writes and reads Excel workbooks (.xlsx). Rows are written and read one at a time, so large files don't need to fit
/// in memory as a document tree. Numbers, booleans, dates and date-times keep their types; when reading, the type of
/// each column is inferred from the values of the first batch.
/// </summary>
public sealed class ExcelDataFileFormat : IDataFileFormat
{
    /// <summary>
    /// The technical name of the format.
    /// </summary>
    public const string FormatName = "xlsx";

    private const uint IntegerStyle = 1;
    private const uint DateStyle = 2;
    private const uint DateTimeStyle = 3;
    private const uint HeaderStyle = 4;
    private const int MaxCellText = 32_767;

    private readonly IStringLocalizer S;

    public ExcelDataFileFormat(IStringLocalizer<ExcelDataFileFormat> localizer)
    {
        S = localizer;
        DisplayName = localizer["Excel workbook"];
    }

    public string Name => FormatName;

    public LocalizedString DisplayName { get; }

    public string Extension => ".xlsx";

    public string ContentType => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

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

        options ??= new DataFileOptions();

        var count = 0L;

        using (var document = SpreadsheetDocument.Create(stream, SpreadsheetDocumentType.Workbook))
        {
            var workbookPart = document.AddWorkbookPart();
            var stylesPart = workbookPart.AddNewPart<WorkbookStylesPart>();
            stylesPart.Stylesheet = BuildStylesheet();

            var worksheetPart = workbookPart.AddNewPart<WorksheetPart>();

            using (var writer = OpenXmlWriter.Create(worksheetPart))
            {
                writer.WriteStartElement(new Worksheet());
                writer.WriteStartElement(new SheetData());

                if (options.HasHeaderRow)
                {
                    writer.WriteStartElement(new Row());

                    foreach (var field in fields)
                    {
                        var name = options.UseDisplayNames && !string.IsNullOrEmpty(field.DisplayName) ? field.DisplayName : field.Name;
                        var cell = TextCell(name);
                        cell.StyleIndex = HeaderStyle;
                        writer.WriteElement(cell);
                    }

                    writer.WriteEndElement();
                }

                await foreach (var batch in batches.WithCancellation(cancellationToken))
                {
                    foreach (var row in batch.Rows)
                    {
                        writer.WriteStartElement(new Row());

                        for (var index = 0; index < fields.Count; index++)
                        {
                            writer.WriteElement(ToCell(index < row.Length ? row[index] : null, fields[index].Type));
                        }

                        writer.WriteEndElement();
                        count++;
                    }
                }

                writer.WriteEndElement();
                writer.WriteEndElement();
            }

            workbookPart.Workbook = new Workbook(
                new Sheets(
                    new Sheet
                    {
                        Id = workbookPart.GetIdOfPart(worksheetPart),
                        SheetId = 1,
                        Name = string.IsNullOrWhiteSpace(options.SheetName) ? "Sheet1" : Truncate(options.SheetName.Trim(), 31),
                    }));
        }

        await stream.FlushAsync(cancellationToken);

        return count;
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
        => value switch
        {
            ExcelDate date => date.HasTime ? DataFieldType.DateTime : DataFieldType.Date,
            long => DataFieldType.Integer,
            decimal => DataFieldType.Decimal,
            bool => DataFieldType.Boolean,
            _ => DataFieldType.Text,
        };

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

    private static Cell ToCell(object value, DataFieldType type)
    {
        if (value is null)
        {
            return new Cell();
        }

        switch (type)
        {
            case DataFieldType.Integer when DataValues.Coerce(value, DataFieldType.Integer) is long number:
                return new Cell { CellValue = new CellValue(number.ToString(CultureInfo.InvariantCulture)), StyleIndex = IntegerStyle };

            case DataFieldType.Decimal when DataValues.Coerce(value, DataFieldType.Decimal) is decimal number:
                return new Cell { CellValue = new CellValue(number.ToString(CultureInfo.InvariantCulture)) };

            case DataFieldType.Boolean when DataValues.Coerce(value, DataFieldType.Boolean) is bool flag:
                return new Cell { CellValue = new CellValue(flag ? "1" : "0"), DataType = CellValues.Boolean };

            case DataFieldType.Date when DataValues.Coerce(value, DataFieldType.Date) is DateTime date:
                return new Cell { CellValue = new CellValue(date.ToOADate().ToString("R", CultureInfo.InvariantCulture)), StyleIndex = DateStyle };

            case DataFieldType.DateTime when DataValues.Coerce(value, DataFieldType.DateTime) is DateTime dateTime:
                return new Cell { CellValue = new CellValue(dateTime.ToOADate().ToString("R", CultureInfo.InvariantCulture)), StyleIndex = DateTimeStyle };

            default:
                return TextCell(DataFileValues.ToText(value, type));
        }
    }

    private static Cell TextCell(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return new Cell();
        }

        return new Cell
        {
            DataType = CellValues.InlineString,
            InlineString = new InlineString(new Text(Truncate(text, MaxCellText)) { Space = SpaceProcessingModeValues.Preserve }),
        };
    }

    private static Stylesheet BuildStylesheet()
        => new(
            new Fonts(
                new Font(),
                new Font(new Bold())),
            new Fills(
                new Fill(new PatternFill { PatternType = PatternValues.None }),
                new Fill(new PatternFill { PatternType = PatternValues.Gray125 })),
            new Borders(new Border()),
            new CellStyleFormats(new CellFormat()),
            new CellFormats(
                new CellFormat(),
                new CellFormat { NumberFormatId = 1, ApplyNumberFormat = true },
                new CellFormat { NumberFormatId = 14, ApplyNumberFormat = true },
                new CellFormat { NumberFormatId = 22, ApplyNumberFormat = true },
                new CellFormat { FontId = 1, ApplyFont = true }));

    private static string EmptyToNull(string value) => string.IsNullOrEmpty(value) ? null : value;

    private static string Truncate(string value, int length) => value.Length <= length ? value : value[..length];

    private enum StyleKind
    {
        General,
        Integer,
        Date,
        DateTime,
    }

    private sealed record ExcelDate(DateTime Value, bool HasTime);
}
