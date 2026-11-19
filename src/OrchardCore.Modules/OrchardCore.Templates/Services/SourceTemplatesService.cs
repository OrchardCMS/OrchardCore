using System.Security.Cryptography;
using System.Text;
using OrchardCore.Templates.Models;

namespace OrchardCore.Templates.Services;

/// <summary>
/// Lists the templates of the template sources with the state of their local overrides, and clones, resets
/// and deletes these overrides.
/// </summary>
public sealed class SourceTemplatesService
{
    private readonly IEnumerable<ITemplateSource> _sources;
    private readonly ShapeTemplateFiles _shapeTemplateFiles;
    private readonly TemplatesManager _templatesManager;
    private readonly AdminTemplatesManager _adminTemplatesManager;
    private readonly TemplateOverridesManager _overridesManager;

    public SourceTemplatesService(
        IEnumerable<ITemplateSource> sources,
        ShapeTemplateFiles shapeTemplateFiles,
        TemplatesManager templatesManager,
        AdminTemplatesManager adminTemplatesManager,
        TemplateOverridesManager overridesManager)
    {
        _sources = sources;
        _shapeTemplateFiles = shapeTemplateFiles;
        _templatesManager = templatesManager;
        _adminTemplatesManager = adminTemplatesManager;
        _overridesManager = overridesManager;
    }

    /// <summary>
    /// Gets the registered template sources.
    /// </summary>
    public IEnumerable<ITemplateSource> Sources => _sources;

    /// <summary>
    /// Gets a template source by its name.
    /// </summary>
    public ITemplateSource GetSource(string name)
        => _sources.LastOrDefault(source => string.Equals(source.Name, name, StringComparison.OrdinalIgnoreCase));

    /// <summary>
    /// Lists the templates of a source, sorted by name, with the state of their local overrides.
    /// </summary>
    /// <param name="source">The source of the templates.</param>
    /// <param name="adminTemplates">Whether to list the admin templates.</param>
    /// <param name="scope">The scope of the source to list, e.g. a theme, or <see langword="null"/> for the one in use.</param>
    public async Task<IReadOnlyList<SourceTemplateEntry>> GetEntriesAsync(ITemplateSource source, bool adminTemplates, string scope)
    {
        var templates = await source.GetTemplatesAsync(adminTemplates, scope);
        var localTemplates = await GetTemplatesDocumentAsync(adminTemplates);
        var overrides = await _overridesManager.GetOverridesDocumentAsync(adminTemplates);

        List<SourceTemplateEntry> entries = [];

        foreach (var template in templates.OrderBy(template => template.Name, StringComparer.OrdinalIgnoreCase))
        {
            var entry = new SourceTemplateEntry
            {
                Source = source.Name,
                Scope = scope,
                Template = template,
                AdminTemplates = adminTemplates,
            };

            if (localTemplates.Templates.TryGetValue(template.Name, out var localTemplate))
            {
                entry.IsOverridden = true;

                // A local template cloned from another file, e.g. of a theme that was active before, is not compared to this one.
                if (overrides.Overrides.TryGetValue(template.Name, out var record) && !IsClonedFrom(record, template))
                {
                    entry.ClonedFrom = record.ExtensionId;
                }
                else if (record != null)
                {
                    entry.IsCloned = true;
                    entry.IsModified = !string.Equals(ComputeHash(localTemplate.Content), record.Hash, StringComparison.Ordinal);

                    var content = await _shapeTemplateFiles.ReadAsync(template);
                    entry.IsSourceChanged = content != null && !string.Equals(ComputeHash(content), record.Hash, StringComparison.Ordinal);
                }
            }

            entries.Add(entry);
        }

        return entries;
    }

    /// <summary>
    /// Gets a template of a source by its name, with the state of its local override.
    /// </summary>
    /// <returns>The entry, or <see langword="null"/> when the source has no such template.</returns>
    public async Task<SourceTemplateEntry> GetEntryAsync(ITemplateSource source, string name, bool adminTemplates, string scope)
        => string.IsNullOrEmpty(name)
            ? null
            : (await GetEntriesAsync(source, adminTemplates, scope))
                .FirstOrDefault(entry => string.Equals(entry.Template.Name, name, StringComparison.OrdinalIgnoreCase));

    /// <summary>
    /// Finds a template of a source by its name.
    /// </summary>
    /// <returns>The template, or <see langword="null"/> when the source has no such template.</returns>
    public static async Task<SourceTemplate> FindTemplateAsync(ITemplateSource source, string name, bool adminTemplates, string scope)
    {
        if (string.IsNullOrEmpty(name))
        {
            return null;
        }

        return (await source.GetTemplatesAsync(adminTemplates, scope))
            .FirstOrDefault(template => string.Equals(template.Name, name, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Finds the templates of the sources that have some names, to view or clone them.
    /// </summary>
    /// <param name="names">The names of the templates.</param>
    /// <param name="adminTemplates">Whether to look for admin templates.</param>
    /// <returns>
    /// The templates by name. The sources are in their registration order, the theme before the features,
    /// as the template of the theme wins when both have one.
    /// </returns>
    public async Task<IReadOnlyDictionary<string, InspectedSourceTemplate>> FindSourceTemplatesAsync(IEnumerable<string> names, bool adminTemplates)
    {
        var lookup = new HashSet<string>(names, StringComparer.OrdinalIgnoreCase);
        var sourceTemplates = new Dictionary<string, InspectedSourceTemplate>(StringComparer.OrdinalIgnoreCase);

        foreach (var source in _sources)
        {
            foreach (var template in await source.GetTemplatesAsync(adminTemplates, null))
            {
                if (lookup.Contains(template.Name))
                {
                    sourceTemplates.TryAdd(template.Name, new InspectedSourceTemplate { Source = source.Name, Template = template });
                }
            }
        }

        return sourceTemplates;
    }

    /// <summary>
    /// Reads the content of a template of a source.
    /// </summary>
    public Task<string> ReadAsync(SourceTemplate template) => _shapeTemplateFiles.ReadAsync(template);

    /// <summary>
    /// Clones a Liquid template in use as a local template, and records where it comes from to monitor it.
    /// </summary>
    /// <param name="template">The template to clone.</param>
    /// <param name="adminTemplates">Whether to clone it as an admin template.</param>
    /// <param name="overwrite">Whether to replace a local template with the same name, which resets it to the source.</param>
    /// <returns>Whether the template was cloned.</returns>
    public async Task<bool> CloneAsync(SourceTemplate template, bool adminTemplates, bool overwrite)
    {
        if (!template.CanClone)
        {
            return false;
        }

        var content = await _shapeTemplateFiles.ReadAsync(template);

        if (string.IsNullOrEmpty(content))
        {
            return false;
        }

        var localTemplates = await LoadTemplatesDocumentAsync(adminTemplates);

        localTemplates.Templates.TryGetValue(template.Name, out var localTemplate);

        if (localTemplate != null && !overwrite)
        {
            return false;
        }

        var overrides = await _overridesManager.LoadOverridesDocumentAsync(adminTemplates);

        // A reset keeps the description the template was given, not the one telling which file it was cloned from.
        var description = localTemplate?.Description;

        if (description == null ||
            (overrides.Overrides.TryGetValue(template.Name, out var record) && description == GetCloneDescription(record.ExtensionId, record.RelativePath)))
        {
            description = GetCloneDescription(template.ExtensionId, template.RelativePath);
        }

        var clone = new Template
        {
            Content = content,
            Description = description,
        };

        await UpdateTemplateAsync(template.Name, clone, adminTemplates);

        overrides.Overrides[template.Name] = new TemplateOverride
        {
            ExtensionId = template.ExtensionId,
            RelativePath = template.RelativePath,
            Hash = ComputeHash(content),
        };

        await _overridesManager.UpdateOverridesDocumentAsync(overrides, adminTemplates);

        return true;
    }

    /// <summary>
    /// Clones a Liquid template in use as a local template with another name, e.g. an alternate of its shape, as a starting
    /// point. It is not monitored, as it is not the template of the source with the same name.
    /// </summary>
    /// <param name="template">The template to clone.</param>
    /// <param name="name">The name of the local template, e.g. <c>Content__BlogPost</c> for <c>Content</c>.</param>
    /// <param name="adminTemplates">Whether to clone it as an admin template.</param>
    /// <returns>Whether the template was cloned, which it is not when a local template has this name.</returns>
    public async Task<bool> CloneAsAsync(SourceTemplate template, string name, bool adminTemplates)
    {
        if (!template.CanClone || string.IsNullOrWhiteSpace(name))
        {
            return false;
        }

        var localTemplates = await LoadTemplatesDocumentAsync(adminTemplates);

        if (localTemplates.Templates.ContainsKey(name))
        {
            return false;
        }

        var content = await _shapeTemplateFiles.ReadAsync(template);

        if (string.IsNullOrEmpty(content))
        {
            return false;
        }

        var clone = new Template
        {
            Content = content,
            Description = $"Cloned from {template.ExtensionId} ({template.RelativePath}) as an alternate of {template.Name}.",
        };

        await UpdateTemplateAsync(name, clone, adminTemplates);

        return true;
    }

    /// <summary>
    /// Deletes the local template overriding a template, so that the template of the source is used again.
    /// </summary>
    /// <returns>Whether a local template was deleted.</returns>
    public async Task<bool> DeleteAsync(string name, bool adminTemplates)
    {
        var localTemplates = await LoadTemplatesDocumentAsync(adminTemplates);

        if (!localTemplates.Templates.ContainsKey(name))
        {
            return false;
        }

        await (adminTemplates
            ? _adminTemplatesManager.RemoveTemplateAsync(name)
            : _templatesManager.RemoveTemplateAsync(name));

        await _overridesManager.ForgetClonesAsync([name], adminTemplates);

        return true;
    }

    private static string GetCloneDescription(string extensionId, string relativePath)
        => $"Cloned from {extensionId} ({relativePath}).";

    private static bool IsClonedFrom(TemplateOverride record, SourceTemplate template)
        => string.Equals(record.ExtensionId, template.ExtensionId, StringComparison.OrdinalIgnoreCase)
            && string.Equals(record.RelativePath, template.RelativePath, StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Computes the hash of a template content, ignoring the line endings, which can change when a file is checked out
    /// or a template is posted from a browser.
    /// </summary>
    public static string ComputeHash(string content)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes((content ?? string.Empty).ReplaceLineEndings("\n"))));

    /// <summary>
    /// Gets the local templates from the cache, for sharing and without any change.
    /// </summary>
    /// <param name="adminTemplates">Whether to get the admin templates.</param>
    public async Task<TemplatesDocument> GetTemplatesDocumentAsync(bool adminTemplates)
        => adminTemplates
            ? await _adminTemplatesManager.GetTemplatesDocumentAsync()
            : await _templatesManager.GetTemplatesDocumentAsync();

    private async Task<TemplatesDocument> LoadTemplatesDocumentAsync(bool adminTemplates)
        => adminTemplates
            ? await _adminTemplatesManager.LoadTemplatesDocumentAsync()
            : await _templatesManager.LoadTemplatesDocumentAsync();

    private Task UpdateTemplateAsync(string name, Template template, bool adminTemplates)
        => adminTemplates
            ? _adminTemplatesManager.UpdateTemplateAsync(name, template)
            : _templatesManager.UpdateTemplateAsync(name, template);
}
