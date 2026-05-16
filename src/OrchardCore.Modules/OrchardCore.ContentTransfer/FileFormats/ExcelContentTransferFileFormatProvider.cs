using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;

namespace OrchardCore.ContentTransfer.FileFormats;

public sealed class ExcelContentTransferFileFormatProvider : IContentTransferFileFormatProvider
{
    public string FileExtension => ".xlsx";

    public string ContentType => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    public bool CanHandle(string fileName)
        => Path.GetExtension(fileName).Equals(FileExtension, StringComparison.OrdinalIgnoreCase);

    public IContentTransferFileReader CreateReader(Stream stream)
        => new ExcelFileReader(stream);

    public IContentTransferFileWriter CreateWriter(Stream stream, string sheetName)
        => new ExcelFileWriter(stream, sheetName);

    private sealed class ExcelFileReader : IContentTransferFileReader
    {
        private readonly SpreadsheetDocument _document;
        private readonly SharedStringTable _sharedStringTable;
        private readonly SheetData _sheetData;
        private readonly List<string> _columnNames;
        private readonly int _rowCount;

        public ExcelFileReader(Stream stream)
        {
            _document = SpreadsheetDocument.Open(stream, false);
            var workbookPart = _document.WorkbookPart
                ?? throw new InvalidOperationException("Unable to read the uploaded file.");

            var firstSheet = workbookPart.Workbook.Descendants<Sheet>().FirstOrDefault()
                ?? throw new InvalidOperationException("Unable to find a tab in the file that contains data.");

            var worksheetPart = (WorksheetPart)workbookPart.GetPartById(firstSheet.Id);
            _sheetData = worksheetPart.Worksheet.GetFirstChild<SheetData>()
                ?? throw new InvalidOperationException("Unable to find a tab in the file that contains data.");

            _sharedStringTable = workbookPart.GetPartsOfType<SharedStringTablePart>()
                .FirstOrDefault()?.SharedStringTable;

            var headerRow = _sheetData.Elements<Row>().FirstOrDefault()
                ?? throw new InvalidOperationException("Unable to find a tab in the file that contains data.");

            var columnNames = new List<string>();
            foreach (var cell in headerRow.Descendants<Cell>())
            {
                var columnName = GetCellValue(cell)?.Trim() ?? string.Empty;
                var columnIndex = GetColumnIndexFromCellReference(cell.CellReference);

                // Pad missing columns.
                while (columnNames.Count < columnIndex)
                {
                    columnNames.Add("Col " + (columnNames.Count + 1));
                }

                if (string.IsNullOrEmpty(columnName))
                {
                    columnName = "Col " + (columnIndex + 1);
                }

                columnNames.Add(columnName);
            }

            _columnNames = columnNames;
            _rowCount = Math.Max(_sheetData.Elements<Row>().Count() - 1, 0);
        }

        public IReadOnlyList<string> GetColumnNames() => _columnNames;

        public int GetRowCount() => _rowCount;

        public IEnumerable<string[]> ReadRows()
        {
            var columnCount = _columnNames.Count;

            foreach (var sheetRow in _sheetData.Elements<Row>().Skip(1))
            {
                var values = new string[columnCount];

                foreach (var cell in sheetRow.Descendants<Cell>())
                {
                    var colIndex = GetColumnIndexFromCellReference(cell.CellReference);

                    if (colIndex < columnCount)
                    {
                        values[colIndex] = GetCellValue(cell);
                    }
                }

                yield return values;
            }
        }

        public void Dispose()
        {
            _document.Dispose();
        }

        private string GetCellValue(Cell cell)
        {
            if (cell.DataType != null && cell.DataType.Value == CellValues.SharedString
                && _sharedStringTable != null)
            {
                if (int.TryParse(cell.InnerText, out var index))
                {
                    return _sharedStringTable.ElementAt(index).InnerText;
                }
            }

            return cell.CellValue?.Text ?? cell.InnerText;
        }

        private static int GetColumnIndexFromCellReference(string cellReference)
        {
            var columnIndex = 0;

            foreach (var ch in cellReference)
            {
                if (char.IsLetter(ch))
                {
                    columnIndex = (columnIndex * 26) + (char.ToUpperInvariant(ch) - 'A' + 1);
                }
                else
                {
                    break;
                }
            }

            return columnIndex - 1;
        }
    }

    private sealed class ExcelFileWriter : IContentTransferFileWriter
    {
        private readonly SpreadsheetDocument _document;
        private readonly SheetData _sheetData;
        private readonly WorkbookPart _workbookPart;
        private uint _currentRowIndex = 1;
        private int _columnCount;

        public ExcelFileWriter(Stream stream, string sheetName)
        {
            _document = SpreadsheetDocument.Create(stream, SpreadsheetDocumentType.Workbook);
            _workbookPart = _document.AddWorkbookPart();
            _workbookPart.Workbook = new Workbook();

            var worksheetPart = _workbookPart.AddNewPart<WorksheetPart>();
            worksheetPart.Worksheet = new Worksheet(new SheetData());

            var sheets = _workbookPart.Workbook.AppendChild(new Sheets());
            var truncatedName = sheetName?.Length > 31 ? sheetName[..31] : sheetName;
            var sheet = new Sheet()
            {
                Id = _workbookPart.GetIdOfPart(worksheetPart),
                SheetId = 1,
                Name = truncatedName,
            };
            sheets.Append(sheet);

            _sheetData = worksheetPart.Worksheet.GetFirstChild<SheetData>();
        }

        public void WriteHeader(IReadOnlyList<string> columnNames)
        {
            _columnCount = columnNames.Count;
            var headerRow = new Row() { RowIndex = _currentRowIndex };
            _sheetData.Append(headerRow);

            for (var i = 0; i < columnNames.Count; i++)
            {
                var cell = new Cell()
                {
                    CellReference = GetCellReference((uint)(i + 1), _currentRowIndex),
                    DataType = CellValues.String,
                    CellValue = new CellValue(columnNames[i]),
                };
                headerRow.Append(cell);
            }

            _currentRowIndex++;
        }

        public void WriteRow(IReadOnlyList<string> values)
        {
            var row = new Row() { RowIndex = _currentRowIndex };
            _sheetData.Append(row);

            for (var i = 0; i < values.Count && i < _columnCount; i++)
            {
                var cell = new Cell()
                {
                    CellReference = GetCellReference((uint)(i + 1), _currentRowIndex),
                    DataType = CellValues.String,
                    CellValue = new CellValue(values[i] ?? string.Empty),
                };
                row.Append(cell);
            }

            _currentRowIndex++;
        }

        public void Flush()
        {
            _workbookPart.Workbook.Save();
        }

        public void Dispose()
        {
            _document.Dispose();
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
}
