using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.Extensions.Localization;

namespace OrchardCore.DataSources.Files;

/// <summary>
/// Writes and reads delimited text files (RFC 4180): values that hold the delimiter, a quote or a line break are
/// quoted, and quotes are doubled. Files are written in UTF-8 with a byte order mark so spreadsheet applications detect
/// the encoding. Every column read is text.
/// </summary>
public sealed class CsvDataFileFormat : IDataFileFormat
{
    /// <summary>
    /// The technical name of the format.
    /// </summary>
    public const string FormatName = "csv";

    private static readonly UTF8Encoding _encoding = new(encoderShouldEmitUTF8Identifier: true);

    public CsvDataFileFormat(IStringLocalizer<CsvDataFileFormat> localizer)
    {
        DisplayName = localizer["CSV (comma separated values)"];
    }

    public string Name => FormatName;

    public LocalizedString DisplayName { get; }

    public string Extension => ".csv";

    public string ContentType => "text/csv";

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

        var delimiter = GetDelimiter(options);
        var count = 0L;

        await using var writer = new StreamWriter(stream, _encoding, bufferSize: 16 * 1024, leaveOpen: true);

        if (options.HasHeaderRow)
        {
            await WriteLineAsync(writer, fields.Select(field => options.UseDisplayNames && !string.IsNullOrEmpty(field.DisplayName) ? field.DisplayName : field.Name), delimiter);
        }

        await foreach (var batch in batches.WithCancellation(cancellationToken))
        {
            foreach (var row in batch.Rows)
            {
                await WriteLineAsync(writer, fields.Select((field, index) => DataFileValues.ToText(index < row.Length ? row[index] : null, field.Type)), delimiter);
                count++;
            }
        }

        await writer.FlushAsync(cancellationToken);

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
        var delimiter = GetDelimiter(options);

        using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, bufferSize: 16 * 1024, leaveOpen: true);
        var parser = new CsvParser(reader, delimiter);

        DataField[] fields = null;
        var rows = new List<object[]>(batchSize);
        var yielded = false;

        while (await parser.ReadRecordAsync(cancellationToken) is { } record)
        {
            if (fields is null)
            {
                var names = options.HasHeaderRow
                    ? record
                    : record.Select((_, index) => (string)null).ToList();

                fields = DataFileValues.UniqueNames(names)
                    .Select(name => new DataField(name, name, DataFieldType.Text))
                    .ToArray();

                if (options.HasHeaderRow)
                {
                    continue;
                }
            }

            var row = new object[fields.Length];

            for (var index = 0; index < row.Length && index < record.Count; index++)
            {
                row[index] = record[index].Length == 0 ? null : record[index];
            }

            rows.Add(row);

            if (rows.Count == batchSize)
            {
                yield return new DataBatch(fields, rows);
                yielded = true;
                rows = new List<object[]>(batchSize);
            }
        }

        // A file with no rows still yields one batch, so its fields are known.
        if (rows.Count > 0 || !yielded)
        {
            yield return new DataBatch(fields ?? [], rows);
        }
    }

    private static char GetDelimiter(DataFileOptions options)
    {
        var delimiter = options.Delimiter;

        if (string.IsNullOrEmpty(delimiter))
        {
            return ',';
        }

        return delimiter is "\\t" or "tab" ? '\t' : delimiter[0];
    }

    private static async Task WriteLineAsync(StreamWriter writer, IEnumerable<string> values, char delimiter)
    {
        var first = true;

        foreach (var value in values)
        {
            if (!first)
            {
                await writer.WriteAsync(delimiter);
            }

            first = false;

            if (string.IsNullOrEmpty(value))
            {
                continue;
            }

            if (NeedsQuotes(value, delimiter))
            {
                await writer.WriteAsync('"');
                await writer.WriteAsync(value.Replace("\"", "\"\"", StringComparison.Ordinal));
                await writer.WriteAsync('"');
            }
            else
            {
                await writer.WriteAsync(value);
            }
        }

        await writer.WriteAsync("\r\n");
    }

    private static bool NeedsQuotes(string value, char delimiter)
    {
        foreach (var character in value)
        {
            if (character == delimiter || character is '"' or '\r' or '\n')
            {
                return true;
            }
        }

        return char.IsWhiteSpace(value[0]) || char.IsWhiteSpace(value[^1]);
    }

    private sealed class CsvParser
    {
        private readonly TextReader _reader;
        private readonly char _delimiter;
        private readonly char[] _buffer = new char[16 * 1024];
        private int _length;
        private int _position;
        private bool _completed;

        public CsvParser(TextReader reader, char delimiter)
        {
            _reader = reader;
            _delimiter = delimiter;
        }

        public async Task<List<string>> ReadRecordAsync(CancellationToken cancellationToken)
        {
            var values = new List<string>();
            var value = new StringBuilder();
            var quoted = false;
            var inQuotes = false;
            var started = false;

            while (true)
            {
                if (_position == _length)
                {
                    if (_completed)
                    {
                        break;
                    }

                    _length = await _reader.ReadAsync(_buffer.AsMemory(), cancellationToken);
                    _position = 0;

                    if (_length == 0)
                    {
                        _completed = true;

                        break;
                    }
                }

                var character = _buffer[_position++];
                started = true;

                if (inQuotes)
                {
                    if (character == '"')
                    {
                        if (await PeekAsync(cancellationToken) == '"')
                        {
                            _position++;
                            value.Append('"');
                        }
                        else
                        {
                            inQuotes = false;
                        }
                    }
                    else
                    {
                        value.Append(character);
                    }

                    continue;
                }

                if (character == '"' && value.Length == 0 && !quoted)
                {
                    inQuotes = true;
                    quoted = true;

                    continue;
                }

                if (character == _delimiter)
                {
                    values.Add(value.ToString());
                    value.Clear();
                    quoted = false;

                    continue;
                }

                if (character is '\r' or '\n')
                {
                    if (character == '\r' && await PeekAsync(cancellationToken) == '\n')
                    {
                        _position++;
                    }

                    if (values.Count == 0 && value.Length == 0 && !quoted)
                    {
                        // Skip blank lines.
                        started = false;

                        continue;
                    }

                    values.Add(value.ToString());

                    return values;
                }

                value.Append(character);
            }

            if (!started && values.Count == 0 && value.Length == 0)
            {
                return null;
            }

            values.Add(value.ToString());

            return values;
        }

        private async Task<int> PeekAsync(CancellationToken cancellationToken)
        {
            if (_position == _length)
            {
                if (_completed)
                {
                    return -1;
                }

                _length = await _reader.ReadAsync(_buffer.AsMemory(), cancellationToken);
                _position = 0;

                if (_length == 0)
                {
                    _completed = true;

                    return -1;
                }
            }

            return _buffer[_position];
        }
    }
}
