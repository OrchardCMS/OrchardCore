namespace OrchardCore.Lucene.Core;

internal static class LuceneIndexNameValidator
{
    internal static bool IsValid(string name) => !string.IsNullOrWhiteSpace(name) && name.Length <= 255 &&
        name is not "." and not ".." && name == name.Trim() && !name.EndsWith('.') &&
        !name.Any(character => char.IsControl(character) || "/\\:*?\"<>|".Contains(character));
}
