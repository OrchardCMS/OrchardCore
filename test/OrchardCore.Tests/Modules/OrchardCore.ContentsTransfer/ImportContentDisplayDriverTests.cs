using System.IO;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;

namespace OrchardCore.Tests.Modules.OrchardCore.ContentsTransfer;

public class ExcelFileValidationTests
{
    private static readonly HashSet<string> _allowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".xlsx",
    };

    [Theory]
    [InlineData("test.xlsx", true)]
    [InlineData("TEST.XLSX", true)]
    [InlineData("file.Xlsx", true)]
    public void ValidateExtension_WithXlsxFile_ShouldBeAllowed(string fileName, bool expected)
    {
        // Arrange
        var extension = Path.GetExtension(fileName);

        // Act
        var isAllowed = _allowedExtensions.Contains(extension);

        // Assert
        Assert.Equal(expected, isAllowed);
    }

    [Theory]
    [InlineData(".csv")]
    [InlineData(".xls")]
    [InlineData(".txt")]
    [InlineData(".pdf")]
    [InlineData(".xml")]
    public void ValidateExtension_WithNonXlsxFile_ShouldNotBeAllowed(string extension)
    {
        // Act
        var isAllowed = _allowedExtensions.Contains(extension);

        // Assert
        Assert.False(isAllowed);
    }
}

public class ExcelFileReaderTests
{
    [Fact]
    public void CreateValidXlsxFile_ShouldBeReadable()
    {
        // Arrange
        var memoryStream = CreateValidXlsxFile(["Column1", "Column2"],
        [
            ["Value1A", "Value1B"],
            ["Value2A", "Value2B"],
        ]);

        // Act
        using var document = SpreadsheetDocument.Open(memoryStream, false);
        var workbookPart = document.WorkbookPart;
        var sheets = workbookPart.Workbook.Descendants<Sheet>().ToList();

        // Assert
        Assert.Single(sheets);
        Assert.NotNull(document);
    }

    [Fact]
    public void CreateValidXlsxFile_ShouldContainExpectedData()
    {
        // Arrange
        var headers = new[] { "Name", "Description" };
        var data = new[]
        {
            new[] { "Item1", "Description1" },
            new[] { "Item2", "Description2" },
        };

        var memoryStream = CreateValidXlsxFile(headers, data);

        // Act
        var result = ReadXlsxFile(memoryStream);

        // Assert
        Assert.Equal(2, result.Columns.Count);
        Assert.Equal("Name", result.Columns[0]);
        Assert.Equal("Description", result.Columns[1]);
        Assert.Equal(2, result.Rows.Count);
        Assert.Equal("Item1", result.Rows[0][0]);
        Assert.Equal("Description1", result.Rows[0][1]);
    }

    [Fact]
    public void ReadXlsxFile_WithEmptyFile_ShouldReturnEmptyResult()
    {
        // Arrange
        var memoryStream = CreateValidXlsxFile([], []);

        // Act
        var result = ReadXlsxFile(memoryStream);

        // Assert
        Assert.Empty(result.Columns);
        Assert.Empty(result.Rows);
    }

    [Fact]
    public void ReadXlsxFile_WithHeadersOnly_ShouldReturnHeadersAndNoRows()
    {
        // Arrange
        var memoryStream = CreateValidXlsxFile(["Header1", "Header2", "Header3"], []);

        // Act
        var result = ReadXlsxFile(memoryStream);

        // Assert
        Assert.Equal(3, result.Columns.Count);
        Assert.Empty(result.Rows);
    }

    [Fact]
    public void GetCellReference_ShouldReturnCorrectReference()
    {
        // Assert
        Assert.Equal("A1", GetCellReference(1, 1));
        Assert.Equal("B2", GetCellReference(2, 2));
        Assert.Equal("Z1", GetCellReference(26, 1));
        Assert.Equal("AA1", GetCellReference(27, 1));
    }

    private static MemoryStream CreateValidXlsxFile(string[] headers, string[][] rows)
    {
        var memoryStream = new MemoryStream();
        using (var document = SpreadsheetDocument.Create(memoryStream, SpreadsheetDocumentType.Workbook))
        {
            var workbookPart = document.AddWorkbookPart();
            workbookPart.Workbook = new Workbook();

            var worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
            worksheetPart.Worksheet = new Worksheet(new SheetData());

            var sheets = workbookPart.Workbook.AppendChild(new Sheets());
            var sheet = new Sheet
            {
                Id = workbookPart.GetIdOfPart(worksheetPart),
                SheetId = 1,
                Name = "Sheet1",
            };
            sheets.Append(sheet);

            var sheetData = worksheetPart.Worksheet.GetFirstChild<SheetData>();

            // Add header row
            if (headers.Length > 0)
            {
                var headerRow = new Row { RowIndex = 1 };
                sheetData.Append(headerRow);

                for (var i = 0; i < headers.Length; i++)
                {
                    var cell = new Cell
                    {
                        CellReference = GetCellReference((uint)i + 1, 1),
                        DataType = CellValues.String,
                        CellValue = new CellValue(headers[i]),
                    };
                    headerRow.Append(cell);
                }
            }

            // Add data rows
            for (var rowIndex = 0; rowIndex < rows.Length; rowIndex++)
            {
                var dataRow = new Row { RowIndex = (uint)(rowIndex + 2) };
                sheetData.Append(dataRow);

                for (var colIndex = 0; colIndex < rows[rowIndex].Length; colIndex++)
                {
                    var cell = new Cell
                    {
                        CellReference = GetCellReference((uint)colIndex + 1, (uint)(rowIndex + 2)),
                        DataType = CellValues.String,
                        CellValue = new CellValue(rows[rowIndex][colIndex]),
                    };
                    dataRow.Append(cell);
                }
            }

            workbookPart.Workbook.Save();
        }

        memoryStream.Seek(0, SeekOrigin.Begin);
        return memoryStream;
    }

    private static (List<string> Columns, List<string[]> Rows) ReadXlsxFile(Stream stream)
    {
        var columns = new List<string>();
        var rows = new List<string[]>();

        using var document = SpreadsheetDocument.Open(stream, false);
        var workbookPart = document.WorkbookPart;
        var sheets = workbookPart.Workbook.Descendants<Sheet>().ToList();

        if (sheets.Count == 0)
        {
            return (columns, rows);
        }

        var worksheetPart = (WorksheetPart)workbookPart.GetPartById(sheets[0].Id);
        var sheetData = worksheetPart.Worksheet.GetFirstChild<SheetData>();
        var allRows = sheetData.Descendants<Row>().ToList();

        if (allRows.Count == 0)
        {
            return (columns, rows);
        }

        // Read header row
        var headerRow = allRows[0];
        foreach (var cell in headerRow.Descendants<Cell>())
        {
            columns.Add(GetCellValue(cell, workbookPart));
        }

        // Read data rows
        for (var i = 1; i < allRows.Count; i++)
        {
            var rowData = new List<string>();
            foreach (var cell in allRows[i].Descendants<Cell>())
            {
                rowData.Add(GetCellValue(cell, workbookPart));
            }
            rows.Add([.. rowData]);
        }

        return (columns, rows);
    }

    private static string GetCellValue(Cell cell, WorkbookPart workbookPart)
    {
        if (cell?.CellValue == null)
        {
            return string.Empty;
        }

        var value = cell.CellValue.Text;

        if (cell.DataType?.Value == CellValues.SharedString)
        {
            var sharedStringTable = workbookPart.GetPartsOfType<SharedStringTablePart>()
                .FirstOrDefault()?.SharedStringTable;

            if (sharedStringTable != null && int.TryParse(value, out var index))
            {
                return sharedStringTable.ElementAt(index).InnerText;
            }
        }

        return value ?? string.Empty;
    }

    private static string GetCellReference(uint columnIndex, uint rowIndex)
    {
        var columnName = string.Empty;
        var dividend = columnIndex;

        while (dividend > 0)
        {
            var modulo = (dividend - 1) % 26;
            columnName = Convert.ToChar(65 + modulo) + columnName;
            dividend = (dividend - modulo) / 26;
        }

        return columnName + rowIndex;
    }
}
