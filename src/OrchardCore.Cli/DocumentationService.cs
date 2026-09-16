namespace OrchardCore.Cli;

internal static class DocumentationService
{
    public static DocumentationSearchOutput Search(DocumentationIndex index, Uri baseUri, string query, int limit = 10)
    {
        ArgumentNullException.ThrowIfNull(index);
        ArgumentNullException.ThrowIfNull(baseUri);
        ArgumentException.ThrowIfNullOrWhiteSpace(query);

        var terms = query.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var results = index.Docs
            .Select(doc => CreateHit(doc, baseUri, terms))
            .Where(hit => hit.Score > 0)
            .OrderByDescending(hit => hit.Score)
            .ThenBy(hit => hit.Title, StringComparer.Ordinal)
            .Take(limit)
            .ToList();

        return new DocumentationSearchOutput
        {
            Query = query,
            Results = results,
        };
    }

    public static DocumentationShowOutput Show(DocumentationIndex index, Uri baseUri, string selector)
    {
        ArgumentNullException.ThrowIfNull(index);
        ArgumentNullException.ThrowIfNull(baseUri);
        ArgumentException.ThrowIfNullOrWhiteSpace(selector);

        var match = int.TryParse(selector, out var id)
            ? index.Docs.FirstOrDefault(doc => doc.Id == id)
            : index.Docs.FirstOrDefault(doc => string.Equals(doc.Location, selector, StringComparison.OrdinalIgnoreCase))
                ?? index.Docs.FirstOrDefault(doc => string.Equals(doc.Title, selector, StringComparison.OrdinalIgnoreCase));

        if (match is null)
        {
            throw new CliException($"No documentation entry matched '{selector}'.");
        }

        return new DocumentationShowOutput
        {
            Id = match.Id,
            Title = match.Title,
            Location = match.Location,
            Url = new Uri(baseUri, match.Location).AbsoluteUri,
            Text = match.Text,
        };
    }

    private static DocumentationSearchHit CreateHit(DocumentationEntry entry, Uri baseUri, IReadOnlyList<string> terms)
    {
        var title = entry.Title;
        var location = entry.Location;
        var text = entry.Text;
        var score = 0;

        foreach (var term in terms)
        {
            if (title.Contains(term, StringComparison.OrdinalIgnoreCase))
            {
                score += 10;
            }

            if (location.Contains(term, StringComparison.OrdinalIgnoreCase))
            {
                score += 5;
            }

            if (text.Contains(term, StringComparison.OrdinalIgnoreCase))
            {
                score += 1;
            }
        }

        return new DocumentationSearchHit
        {
            Id = entry.Id,
            Title = title,
            Location = location,
            Url = new Uri(baseUri, location).AbsoluteUri,
            Snippet = text.Length <= 180 ? text : $"{text[..177]}...",
            Score = score,
        };
    }
}
