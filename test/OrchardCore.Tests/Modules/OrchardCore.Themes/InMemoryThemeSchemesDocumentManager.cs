using OrchardCore.Documents;
using OrchardCore.Themes.Schemes.Models;

namespace OrchardCore.Tests.Modules.OrchardCore.Themes;

/// <summary>
/// The schemes document, kept in memory; each update gives it a new identifier, as the document store does.
/// </summary>
public sealed class InMemoryThemeSchemesDocumentManager : IDocumentManager<ThemeSchemesDocument>
{
    public ThemeSchemesDocument Document { get; private set; } = new();

    public int Updates { get; private set; }

    public Task<ThemeSchemesDocument> GetOrCreateMutableAsync(Func<Task<ThemeSchemesDocument>> factoryAsync = null)
        => Task.FromResult(Document);

    public Task<ThemeSchemesDocument> GetOrCreateImmutableAsync(Func<Task<ThemeSchemesDocument>> factoryAsync = null)
        => Task.FromResult(Document);

    public Task UpdateAsync(ThemeSchemesDocument document, Func<ThemeSchemesDocument, Task> afterUpdateAsync = null)
    {
        document.Identifier = IdGenerator.GenerateId();
        Document = document;
        Updates++;

        return Task.CompletedTask;
    }
}
