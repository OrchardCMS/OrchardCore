using System.Data;
using System.IO;
using System.Linq;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;

namespace OrchardCore.Tests.Modules.OrchardCore.ContentsTransfer;

public class ExcelImportTests
{
    [Fact]
    public void CanReadExcelFile()
    {
        // Arrange
        var memoryStream = CreateTestExcelFile(new[]
        {
            new[] { "Name", "Value", "Description" },
            new[] { "Item1", "100", "Test description 1" },
            new[] { "Item2", "200", "Test description 2" },
        });

        // Act
        var dataTable = ReadExcelFile(memoryStream);

        // Assert
        Assert.Equal(3, dataTable.Columns.Count);
        Assert.Equal(2, dataTable.Rows.Count);
        Assert.Equal("Name", dataTable.Columns[0].ColumnName);
        Assert.Equal("Value", dataTable.Columns[1].ColumnName);
        Assert.Equal("Description", dataTable.Columns[2].ColumnName);
        Assert.Equal("Item1", dataTable.Rows[0][0]);
        Assert.Equal("100", dataTable.Rows[0][1]);
        Assert.Equal("Test description 1", dataTable.Rows[0][2]);
    }

    [Fact]
    public void CanReadExcelFileWithEmptyCells()
    {
        // Arrange
        var memoryStream = CreateTestExcelFile(new[]
        {
            new[] { "Name", "Value", "Description" },
            new[] { "Item1", "", "Test description 1" },
            new[] { "Item2", "200", "" },
        });

        // Act
        var dataTable = ReadExcelFile(memoryStream);

        // Assert
        Assert.Equal(3, dataTable.Columns.Count);
        Assert.Equal(2, dataTable.Rows.Count);
        Assert.Equal("Item1", dataTable.Rows[0][0]);
        Assert.Equal(string.Empty, dataTable.Rows[0][1]);
        Assert.Equal("Test description 1", dataTable.Rows[0][2]);
        Assert.Equal("Item2", dataTable.Rows[1][0]);
        Assert.Equal("200", dataTable.Rows[1][1]);
        Assert.Equal(string.Empty, dataTable.Rows[1][2]);
    }

    [Fact]
    public void CanCreateExcelFile()
    {
        // Arrange
        var dataTable = new DataTable();
        dataTable.Columns.Add("Name");
        dataTable.Columns.Add("Value");
        dataTable.Columns.Add("Description");

        var row1 = dataTable.NewRow();
        row1["Name"] = "Item1";
        row1["Value"] = "100";
        row1["Description"] = "Test description 1";
        dataTable.Rows.Add(row1);

        var row2 = dataTable.NewRow();
        row2["Name"] = "Item2";
        row2["Value"] = "200";
        row2["Description"] = "Test description 2";
        dataTable.Rows.Add(row2);

        // Act
        var memoryStream = CreateExcelFile(dataTable, "TestSheet");

        // Assert
        var resultDataTable = ReadExcelFile(memoryStream);
        Assert.Equal(3, resultDataTable.Columns.Count);
        Assert.Equal(2, resultDataTable.Rows.Count);
        Assert.Equal("Name", resultDataTable.Columns[0].ColumnName);
        Assert.Equal("Item1", resultDataTable.Rows[0][0]);
        Assert.Equal("Item2", resultDataTable.Rows[1][0]);
    }

    [Fact]
    public void CanHandleDuplicateColumnNames()
    {
        // Arrange
        var memoryStream = CreateTestExcelFile(new[]
        {
            new[] { "Name", "Name", "Value" },
            new[] { "Item1", "Duplicate1", "100" },
        });

        // Act
        var dataTable = ReadExcelFile(memoryStream);

        // Assert
        Assert.Equal(3, dataTable.Columns.Count);
        Assert.Equal("Name", dataTable.Columns[0].ColumnName);
        Assert.Equal("Name 2", dataTable.Columns[1].ColumnName);
        Assert.Equal("Value", dataTable.Columns[2].ColumnName);
    }

    [Fact]
    public void ReturnsEmptyDataTableForEmptyFile()
    {
        // Arrange
        var memoryStream = CreateTestExcelFile(Array.Empty<string[]>());

        // Act
        var dataTable = ReadExcelFile(memoryStream);

        // Assert
        Assert.Equal(0, dataTable.Columns.Count);
        Assert.Equal(0, dataTable.Rows.Count);
    }

    private static MemoryStream CreateTestExcelFile(string[][] data)
    {
        var memoryStream = new MemoryStream();

        using (var spreadsheetDocument = SpreadsheetDocument.Create(memoryStream, SpreadsheetDocumentType.Workbook))
        {
            var workbookPart = spreadsheetDocument.AddWorkbookPart();
            workbookPart.Workbook = new Workbook();

            var worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
            var sheetData = new SheetData();
            worksheetPart.Worksheet = new Worksheet(sheetData);

            var sheets = workbookPart.Workbook.AppendChild(new Sheets());
            var sheet = new Sheet
            {
                Id = workbookPart.GetIdOfPart(worksheetPart),
                SheetId = 1,
                Name = "TestSheet",
            };
            sheets.Append(sheet);

            uint rowIndex = 1;
            foreach (var rowData in data)
            {
                var row = new Row { RowIndex = rowIndex };
                uint columnIndex = 1;
                foreach (var cellData in rowData)
                {
                    var cell = new Cell
                    {
                        CellReference = GetCellReference(columnIndex, rowIndex),
                        DataType = CellValues.String,
                        CellValue = new CellValue(cellData ?? string.Empty),
                    };
                    row.Append(cell);
                    columnIndex++;
                }
                sheetData.Append(row);
                rowIndex++;
            }

            workbookPart.Workbook.Save();
        }

        memoryStream.Position = 0;
        return memoryStream;
    }

    private static MemoryStream CreateExcelFile(DataTable dataTable, string sheetName)
    {
        var memoryStream = new MemoryStream();

        using (var spreadsheetDocument = SpreadsheetDocument.Create(memoryStream, SpreadsheetDocumentType.Workbook))
        {
            var workbookPart = spreadsheetDocument.AddWorkbookPart();
            workbookPart.Workbook = new Workbook();

            var worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
            var sheetData = new SheetData();
            worksheetPart.Worksheet = new Worksheet(sheetData);

            var sheets = workbookPart.Workbook.AppendChild(new Sheets());
            var sheet = new Sheet
            {
                Id = workbookPart.GetIdOfPart(worksheetPart),
                SheetId = 1,
                Name = sheetName,
            };
            sheets.Append(sheet);

            // Create header row
            var headerRow = new Row { RowIndex = 1 };
            uint columnIndex = 1;
            foreach (DataColumn column in dataTable.Columns)
            {
                var cell = new Cell
                {
                    CellReference = GetCellReference(columnIndex, 1),
                    DataType = CellValues.String,
                    CellValue = new CellValue(column.ColumnName),
                };
                headerRow.Append(cell);
                columnIndex++;
            }
            sheetData.Append(headerRow);

            // Create data rows
            uint rowIndex = 2;
            foreach (DataRow dataRow in dataTable.Rows)
            {
                var row = new Row { RowIndex = rowIndex };
                columnIndex = 1;
                foreach (var item in dataRow.ItemArray)
                {
                    var cell = new Cell
                    {
                        CellReference = GetCellReference(columnIndex, rowIndex),
                        DataType = CellValues.String,
                        CellValue = new CellValue(item?.ToString() ?? string.Empty),
                    };
                    row.Append(cell);
                    columnIndex++;
                }
                sheetData.Append(row);
                rowIndex++;
            }

            workbookPart.Workbook.Save();
        }

        memoryStream.Position = 0;
        return memoryStream;
    }

    private static DataTable ReadExcelFile(Stream stream)
    {
        var dataTable = new DataTable();
        stream.Position = 0;

        using var spreadsheetDocument = SpreadsheetDocument.Open(stream, false);
        var workbookPart = spreadsheetDocument.WorkbookPart;

        if (workbookPart == null)
        {
            return dataTable;
        }

        var sheets = workbookPart.Workbook.Sheets;
        if (sheets == null || !sheets.Elements<Sheet>().Any())
        {
            return dataTable;
        }

        var sheet = sheets.Elements<Sheet>().FirstOrDefault();
        if (sheet?.Id?.Value == null)
        {
            return dataTable;
        }

        var worksheetPart = (WorksheetPart)workbookPart.GetPartById(sheet.Id.Value);
        var sheetData = worksheetPart.Worksheet.Elements<SheetData>().FirstOrDefault();

        if (sheetData == null || !sheetData.Elements<Row>().Any())
        {
            return dataTable;
        }

        var sharedStringTable = workbookPart.SharedStringTablePart?.SharedStringTable;
        var rows = sheetData.Elements<Row>().ToList();

        if (rows.Count == 0)
        {
            return dataTable;
        }

        // Process header row
        var headerRow = rows[0];
        var columnNames = new List<string>();
        var maxColumnIndex = 0;

        foreach (var cell in headerRow.Elements<Cell>())
        {
            var columnIndex = GetColumnIndex(cell.CellReference?.Value);
            if (columnIndex > maxColumnIndex)
            {
                maxColumnIndex = columnIndex;
            }
        }

        // Add columns to dataTable
        for (var i = 0; i < maxColumnIndex; i++)
        {
            var columnName = $"Col{i + 1}";
            columnNames.Add(columnName);
            dataTable.Columns.Add(columnName);
        }

        // Update column names from header cells
        foreach (var cell in headerRow.Elements<Cell>())
        {
            var columnIndex = GetColumnIndex(cell.CellReference?.Value) - 1;
            if (columnIndex >= 0 && columnIndex < dataTable.Columns.Count)
            {
                var cellValue = GetCellValue(cell, sharedStringTable)?.Trim() ?? string.Empty;
                if (!string.IsNullOrEmpty(cellValue))
                {
                    var occurrences = columnNames.Take(columnIndex + 1).Count(x => x.Equals(cellValue, StringComparison.OrdinalIgnoreCase));
                    if (occurrences > 0)
                    {
                        var existingIndex = columnNames.IndexOf(cellValue);
                        if (existingIndex >= 0 && existingIndex < columnIndex)
                        {
                            cellValue += " " + (occurrences + 1);
                        }
                    }
                    columnNames[columnIndex] = cellValue;
                    dataTable.Columns[columnIndex].ColumnName = cellValue;
                }
            }
        }

        // Process data rows
        for (var rowIndex = 1; rowIndex < rows.Count; rowIndex++)
        {
            var row = rows[rowIndex];
            var newRow = dataTable.NewRow();

            foreach (var cell in row.Elements<Cell>())
            {
                var columnIndex = GetColumnIndex(cell.CellReference?.Value) - 1;
                if (columnIndex >= 0 && columnIndex < dataTable.Columns.Count)
                {
                    var cellValue = GetCellValue(cell, sharedStringTable);
                    newRow[columnIndex] = cellValue ?? string.Empty;
                }
            }

            dataTable.Rows.Add(newRow);
        }

        return dataTable;
    }

    private static string GetCellValue(Cell cell, SharedStringTable? sharedStringTable)
    {
        if (cell?.CellValue == null)
        {
            return string.Empty;
        }

        var value = cell.CellValue.Text;

        if (cell.DataType?.Value == CellValues.SharedString && sharedStringTable != null)
        {
            if (int.TryParse(value, out var sharedStringIndex))
            {
                var sharedStringItem = sharedStringTable.Elements<SharedStringItem>().ElementAtOrDefault(sharedStringIndex);
                return sharedStringItem?.InnerText ?? string.Empty;
            }
        }

        return value ?? string.Empty;
    }

    private static string GetCellReference(uint columnIndex, uint rowIndex)
    {
        var columnName = string.Empty;
        var tempColumnIndex = columnIndex;

        while (tempColumnIndex > 0)
        {
            var mod = (tempColumnIndex - 1) % 26;
            columnName = Convert.ToChar('A' + mod) + columnName;
            tempColumnIndex = (tempColumnIndex - mod) / 26;
        }

        return columnName + rowIndex;
    }

    private static int GetColumnIndex(string? cellReference)
    {
        if (string.IsNullOrEmpty(cellReference))
        {
            return 0;
        }

        var columnPart = new string(cellReference.TakeWhile(char.IsLetter).ToArray());
        var columnIndex = 0;

        foreach (var c in columnPart.ToUpperInvariant())
        {
            columnIndex = columnIndex * 26 + (c - 'A' + 1);
        }

        return columnIndex;
    }
}
