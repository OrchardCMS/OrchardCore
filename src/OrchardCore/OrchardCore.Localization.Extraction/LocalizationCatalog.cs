namespace OrchardCore.Localization.Extraction;

public sealed record SourceReference(string Path, int Line = 0);

public sealed record ExtractionDiagnostic(string Code, string Message, SourceReference Source, bool IsError = false);

public sealed class LocalizationMessage
{
    public required string Context { get; init; }
    public required string Text { get; init; }
    public string? Plural { get; init; }
    public IList<SourceReference> References { get; } = new List<SourceReference>();
    public ISet<string> Comments { get; } = new SortedSet<string>(StringComparer.Ordinal);
    public ISet<string> Flags { get; } = new SortedSet<string>(StringComparer.Ordinal);
    public IDictionary<int, ISet<string>> FormatArguments { get; } = new SortedDictionary<int, ISet<string>>();
}

public sealed class LocalizationCatalog
{
    private readonly Dictionary<(string Context, string Text), LocalizationMessage> _messages = [];

    public IReadOnlyCollection<LocalizationMessage> Messages => _messages.Values;
    public IList<ExtractionDiagnostic> Diagnostics { get; } = new List<ExtractionDiagnostic>();

    public void Add(string context, string text, string? plural, SourceReference source, string? comment = null, IReadOnlyList<string?>? formatArguments = null)
    {
        if (text.Length == 0)
        {
            Diagnostics.Add(new ExtractionDiagnostic("OCLOC004", "An empty localization key is reserved for the gettext header.", source));
            return;
        }

        var key = (context, text);
        if (_messages.TryGetValue(key, out var existing))
        {
            if (!string.Equals(existing.Plural, plural, StringComparison.Ordinal))
            {
                Diagnostics.Add(new ExtractionDiagnostic("OCLOC005", "Conflicting singular/plural definitions for the same localization context and key.", source, true));
                return;
            }
        }
        else
        {
            existing = new LocalizationMessage { Context = context, Text = text, Plural = plural };
            _messages.Add(key, existing);
        }

        if (!existing.References.Contains(source))
        {
            existing.References.Add(source);
        }

        if (!string.IsNullOrWhiteSpace(comment))
        {
            existing.Comments.Add(comment);
        }

        if (formatArguments is not null)
        {
            for (var index = 0; index < formatArguments.Count; index++)
            {
                if (string.IsNullOrWhiteSpace(formatArguments[index]))
                {
                    continue;
                }

                if (!existing.FormatArguments.TryGetValue(index, out var descriptions))
                {
                    descriptions = new SortedSet<string>(StringComparer.Ordinal);
                    existing.FormatArguments.Add(index, descriptions);
                }

                descriptions.Add(formatArguments[index]!);
            }
        }
    }
}
