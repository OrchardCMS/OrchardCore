using System.Text;

namespace OrchardCore.Cli;

// Retain a bounded tail of child output for failures, without exposing successful
// setup's internal listener or interleaving SDK output with progress messages.
internal sealed class InstallProcessLog : TextWriter
{
    internal const int Capacity = 65536;
    private readonly TextWriter _output;
    private readonly bool _verbose;
    private readonly Queue<string> _lines = new();
    private readonly object _lock = new();
    private int _length;
    private bool _truncated;

    public InstallProcessLog(TextWriter output, bool verbose)
    {
        _output = TextWriter.Synchronized(output);
        _verbose = verbose;
    }

    public override Encoding Encoding => _output.Encoding;

    public override Task WriteLineAsync(string? value)
    {
        if (_verbose)
        {
            return _output.WriteLineAsync(value);
        }

        var line = value ?? string.Empty;
        lock (_lock)
        {
            if (line.Length >= Capacity)
            {
                line = line[^(Capacity - 1)..];
                _truncated = true;
            }

            while (_length + line.Length + 1 > Capacity)
            {
                _length -= _lines.Dequeue().Length + 1;
                _truncated = true;
            }

            _lines.Enqueue(line);
            _length += line.Length + 1;
        }

        return Task.CompletedTask;
    }

    // Called only after the child process and both output readers have stopped.
    public async Task WriteFailureAsync()
    {
        if (_verbose || _lines.Count == 0)
        {
            return;
        }

        await _output.WriteLineAsync(_truncated ? "Recent installation diagnostics (earlier output omitted):" : "Installation diagnostics:");
        foreach (var line in _lines)
        {
            await _output.WriteLineAsync(line);
        }
    }
}
