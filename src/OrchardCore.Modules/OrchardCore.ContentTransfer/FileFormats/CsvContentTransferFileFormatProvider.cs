using System.Text;

namespace OrchardCore.ContentTransfer.FileFormats;

public sealed class CsvContentTransferFileFormatProvider : IContentTransferFileFormatProvider
{
    public string FileExtension => ".csv";

    public string ContentType => "text/csv";

    public bool CanHandle(string fileName)
        => Path.GetExtension(fileName).Equals(FileExtension, StringComparison.OrdinalIgnoreCase);

    public IContentTransferFileReader CreateReader(Stream stream)
        => new CsvFileReader(stream);

    public IContentTransferFileWriter CreateWriter(Stream stream, string sheetName)
        => new CsvFileWriter(stream);

    private sealed class CsvFileReader : IContentTransferFileReader
    {
        private readonly StreamReader _reader;
        private readonly List<string> _columnNames;
        private readonly List<string[]> _rows;

        public CsvFileReader(Stream stream)
        {
            _reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, leaveOpen: true);

            var headerLine = ReadLogicalLine(_reader)
                ?? throw new InvalidOperationException("The CSV file is empty.");

            _columnNames = ParseLine(headerLine);

            // Read all rows into memory for GetRowCount() support.
            _rows = [];
            string line;

            while ((line = ReadLogicalLine(_reader)) != null)
            {
                var values = ParseLine(line);
                var row = new string[_columnNames.Count];

                for (var i = 0; i < Math.Min(values.Count, row.Length); i++)
                {
                    row[i] = values[i];
                }

                _rows.Add(row);
            }
        }

        public IReadOnlyList<string> GetColumnNames() => _columnNames;

        public int GetRowCount() => _rows.Count;

        public IEnumerable<string[]> ReadRows() => _rows;

        public void Dispose()
        {
            _reader.Dispose();
        }

        /// <summary>
        /// Reads a logical CSV line that may span multiple physical lines due to quoted fields.
        /// </summary>
        private static string ReadLogicalLine(StreamReader reader)
        {
            var firstLine = reader.ReadLine();
            if (firstLine == null)
            {
                return null;
            }

            // Fast path: if the line has an even number of quotes, it's a complete logical line.
            if (CountQuotes(firstLine) % 2 == 0)
            {
                return firstLine;
            }

            // Slow path: the line has an unclosed quote, so we need to read more lines.
            var buffer = new StringBuilder();
            buffer.Append(firstLine);

            while (true)
            {
                var nextLine = reader.ReadLine();
                if (nextLine == null)
                {
                    break;
                }

                buffer.Append('\n');
                buffer.Append(nextLine);

                if (CountQuotes(buffer.ToString()) % 2 == 0)
                {
                    break;
                }
            }

            return buffer.ToString();
        }

        private static int CountQuotes(string line)
        {
            var count = 0;
            foreach (var ch in line)
            {
                if (ch == '"')
                {
                    count++;
                }
            }

            return count;
        }

        /// <summary>
        /// Parses a logical CSV line into fields per RFC 4180.
        /// </summary>
        private static List<string> ParseLine(string line)
        {
            var fields = new List<string>();
            var i = 0;

            while (i <= line.Length)
            {
                if (i == line.Length)
                {
                    // Trailing comma means an empty field at end.
                    if (line.Length > 0 && line[^1] == ',')
                    {
                        fields.Add(string.Empty);
                    }

                    break;
                }

                if (line[i] == '"')
                {
                    // Quoted field.
                    var sb = new StringBuilder();
                    i++; // Skip opening quote.

                    while (i < line.Length)
                    {
                        if (line[i] == '"')
                        {
                            if (i + 1 < line.Length && line[i + 1] == '"')
                            {
                                // Escaped quote.
                                sb.Append('"');
                                i += 2;
                            }
                            else
                            {
                                // End of quoted field.
                                i++; // Skip closing quote.
                                break;
                            }
                        }
                        else
                        {
                            sb.Append(line[i]);
                            i++;
                        }
                    }

                    fields.Add(sb.ToString());

                    // Skip comma after quoted field.
                    if (i < line.Length && line[i] == ',')
                    {
                        i++;
                    }
                }
                else
                {
                    // Unquoted field.
                    var commaIndex = line.IndexOf(',', i);
                    if (commaIndex == -1)
                    {
                        fields.Add(line[i..]);
                        break;
                    }
                    else
                    {
                        fields.Add(line[i..commaIndex]);
                        i = commaIndex + 1;
                    }
                }
            }

            return fields;
        }
    }

    private sealed class CsvFileWriter : IContentTransferFileWriter
    {
        private readonly StreamWriter _writer;

        public CsvFileWriter(Stream stream)
        {
            _writer = new StreamWriter(stream, new UTF8Encoding(encoderShouldEmitUTF8Identifier: true), leaveOpen: true);
        }

        public void WriteHeader(IReadOnlyList<string> columnNames)
        {
            WriteLine(columnNames);
        }

        public void WriteRow(IReadOnlyList<string> values)
        {
            WriteLine(values);
        }

        public void Flush()
        {
            _writer.Flush();
        }

        public void Dispose()
        {
            _writer.Dispose();
        }

        private void WriteLine(IReadOnlyList<string> values)
        {
            for (var i = 0; i < values.Count; i++)
            {
                if (i > 0)
                {
                    _writer.Write(',');
                }

                var value = values[i] ?? string.Empty;

                if (NeedsQuoting(value))
                {
                    _writer.Write('"');
                    _writer.Write(value.Replace("\"", "\"\""));
                    _writer.Write('"');
                }
                else
                {
                    _writer.Write(value);
                }
            }

            _writer.WriteLine();
        }

        private static bool NeedsQuoting(string value)
            => value.Contains(',') || value.Contains('"') || value.Contains('\n') || value.Contains('\r');
    }
}
