using OrchardCore.Documents;
using OrchardCore.Themes.TheAdmin.Models;

namespace OrchardCore.Tests.Themes.TheAdmin;

/// <summary>
/// The schemes document, kept in memory; each update gives it a new identifier, as the document store does.
/// </summary>
public sealed class InMemoryAdminSchemesDocumentManager : IDocumentManager<AdminSchemesDocument>
{
    public InMemoryAdminSchemesDocumentManager(AdminSchemesDocument document = null)
    {
        Document = document ?? new AdminSchemesDocument();
    }

    public AdminSchemesDocument Document { get; private set; }

    public int Updates { get; private set; }

    public Task<AdminSchemesDocument> GetOrCreateMutableAsync(Func<Task<AdminSchemesDocument>> factoryAsync = null)
        => Task.FromResult(Document);

    public Task<AdminSchemesDocument> GetOrCreateImmutableAsync(Func<Task<AdminSchemesDocument>> factoryAsync = null)
        => Task.FromResult(Document);

    public Task UpdateAsync(AdminSchemesDocument document, Func<AdminSchemesDocument, Task> afterUpdateAsync = null)
    {
        document.Identifier = IdGenerator.GenerateId();
        Document = document;
        Updates++;

        return Task.CompletedTask;
    }
}
