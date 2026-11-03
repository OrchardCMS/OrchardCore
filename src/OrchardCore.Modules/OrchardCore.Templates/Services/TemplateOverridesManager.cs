using OrchardCore.Documents;
using OrchardCore.Templates.Models;

namespace OrchardCore.Templates.Services;

/// <summary>
/// Manages the records of the local templates cloned from a template source. It belongs to the templates rather than to
/// the Assisted Templates feature, so that a template deleted or renamed while that feature is disabled loses its record.
/// </summary>
public sealed class TemplateOverridesManager
{
    private readonly IDocumentManager<TemplateOverridesDocument> _overridesManager;
    private readonly IDocumentManager<AdminTemplateOverridesDocument> _adminOverridesManager;

    public TemplateOverridesManager(
        IDocumentManager<TemplateOverridesDocument> overridesManager,
        IDocumentManager<AdminTemplateOverridesDocument> adminOverridesManager)
    {
        _overridesManager = overridesManager;
        _adminOverridesManager = adminOverridesManager;
    }

    /// <summary>
    /// Gets the clone records from the cache, for sharing and without any change.
    /// </summary>
    /// <param name="adminTemplates">Whether to get the records of the admin templates.</param>
    public async Task<TemplateOverridesDocument> GetOverridesDocumentAsync(bool adminTemplates)
        => adminTemplates
            ? await _adminOverridesManager.GetOrCreateImmutableAsync()
            : await _overridesManager.GetOrCreateImmutableAsync();

    /// <summary>
    /// Loads the clone records from the store, for updating them.
    /// </summary>
    /// <param name="adminTemplates">Whether to load the records of the admin templates.</param>
    public async Task<TemplateOverridesDocument> LoadOverridesDocumentAsync(bool adminTemplates)
        => adminTemplates
            ? await _adminOverridesManager.GetOrCreateMutableAsync()
            : await _overridesManager.GetOrCreateMutableAsync();

    /// <summary>
    /// Updates the clone records.
    /// </summary>
    /// <param name="document">The records, as loaded by <see cref="LoadOverridesDocumentAsync"/>.</param>
    /// <param name="adminTemplates">Whether they are the records of the admin templates.</param>
    public Task UpdateOverridesDocumentAsync(TemplateOverridesDocument document, bool adminTemplates)
        => adminTemplates
            ? _adminOverridesManager.UpdateAsync((AdminTemplateOverridesDocument)document)
            : _overridesManager.UpdateAsync(document);

    /// <summary>
    /// Forgets where local templates were cloned from, once they are deleted or renamed, so that a template
    /// created later with one of these names is not taken for a clone.
    /// </summary>
    /// <param name="names">The names of the local templates.</param>
    /// <param name="adminTemplates">Whether they are admin templates.</param>
    public async Task ForgetClonesAsync(IEnumerable<string> names, bool adminTemplates)
    {
        var overrides = await LoadOverridesDocumentAsync(adminTemplates);
        var removed = false;

        foreach (var name in names)
        {
            removed |= overrides.Overrides.Remove(name);
        }

        if (removed)
        {
            await UpdateOverridesDocumentAsync(overrides, adminTemplates);
        }
    }
}
