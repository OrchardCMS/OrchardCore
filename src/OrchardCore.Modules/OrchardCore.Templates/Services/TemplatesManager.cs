using OrchardCore.Documents;
using OrchardCore.Templates.Models;

namespace OrchardCore.Templates.Services;

public class TemplatesManager
{
    private readonly IDocumentManager<TemplatesDocument> _documentManager;

    public TemplatesManager(IDocumentManager<TemplatesDocument> documentManager) => _documentManager = documentManager;

    /// <summary>
    /// Loads the templates document from the store for updating and that should not be cached.
    /// </summary>
    public Task<TemplatesDocument> LoadTemplatesDocumentAsync() => _documentManager.GetOrCreateMutableAsync();

    /// <summary>
    /// Gets the templates document from the cache for sharing and that should not be updated.
    /// </summary>
    public Task<TemplatesDocument> GetTemplatesDocumentAsync() => _documentManager.GetOrCreateImmutableAsync();

    public async Task RemoveTemplateAsync(string name)
    {
        var document = await LoadTemplatesDocumentAsync();
        document.Templates.Remove(name);
        await _documentManager.UpdateAsync(document);
    }

    public async Task UpdateTemplateAsync(string name, Template template)
    {
        var document = await LoadTemplatesDocumentAsync();
        document.Templates[name] = template;
        await _documentManager.UpdateAsync(document);
    }

    internal async Task<TemplateCreateResult> CreateTemplateAsync(string name, Template template)
    {
        var document = await LoadTemplatesDocumentAsync();
        if (TryGetTemplate(document, name, out var existingName, out var existingTemplate))
        {
            var status = AreEquivalent(existingTemplate, template)
                ? TemplateCreateStatus.Existing
                : TemplateCreateStatus.Conflict;

            return new TemplateCreateResult(status, existingName, existingTemplate);
        }

        document.Templates[name] = template;
        await _documentManager.UpdateAsync(document);

        return new TemplateCreateResult(TemplateCreateStatus.Created, name, template);
    }

    internal async Task<TemplateMutationResult> ReplaceTemplateAsync(string name, Template template)
    {
        var document = await LoadTemplatesDocumentAsync();
        if (!TryGetTemplate(document, name, out var existingName, out _))
        {
            return null;
        }

        document.Templates[existingName] = template;
        await _documentManager.UpdateAsync(document);

        return new TemplateMutationResult(existingName, template);
    }

    internal async Task<TemplateMutationResult> RemoveTemplateIfExistsAsync(string name)
    {
        var document = await LoadTemplatesDocumentAsync();
        if (!TryGetTemplate(document, name, out var existingName, out var existingTemplate))
        {
            return null;
        }

        document.Templates.Remove(existingName);
        await _documentManager.UpdateAsync(document);

        return new TemplateMutationResult(existingName, existingTemplate);
    }

    private static bool TryGetTemplate(
        TemplatesDocument document,
        string name,
        out string existingName,
        out Template existingTemplate)
    {
        var entry = document.Templates.FirstOrDefault(entry =>
            string.Equals(entry.Key, name, StringComparison.OrdinalIgnoreCase));

        existingName = entry.Key;
        existingTemplate = entry.Value;
        return entry.Key is not null;
    }

    private static bool AreEquivalent(Template left, Template right) =>
        string.Equals(left.Content, right.Content, StringComparison.Ordinal)
        && string.Equals(left.Description, right.Description, StringComparison.Ordinal);
}

internal enum TemplateCreateStatus
{
    Created,
    Existing,
    Conflict,
}

internal sealed class TemplateCreateResult
{
    public TemplateCreateResult(TemplateCreateStatus status, string name, Template template)
    {
        Status = status;
        Name = name;
        Template = template;
    }

    public TemplateCreateStatus Status { get; }

    public string Name { get; }

    public Template Template { get; }
}

internal sealed class TemplateMutationResult
{
    public TemplateMutationResult(string name, Template template)
    {
        Name = name;
        Template = template;
    }

    public string Name { get; }

    public Template Template { get; }
}
