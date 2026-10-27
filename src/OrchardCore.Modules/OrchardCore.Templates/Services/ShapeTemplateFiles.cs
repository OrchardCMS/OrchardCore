using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.DisplayManagement.Descriptors.ShapeTemplateStrategy;
using OrchardCore.Environment.Extensions;
using OrchardCore.Templates.Models;

namespace OrchardCore.Templates.Services;

/// <summary>
/// Lists the shape template files that render the shapes of a theme, from its shape table, so that a template source
/// only decides which of them it lists.
/// </summary>
public sealed class ShapeTemplateFiles
{
    private readonly IShapeTableManager _shapeTableManager;
    private readonly IExtensionManager _extensionManager;
    private readonly IShapeTemplateFileProviderAccessor _fileProviderAccessor;

    public ShapeTemplateFiles(
        IShapeTableManager shapeTableManager,
        IExtensionManager extensionManager,
        IShapeTemplateFileProviderAccessor fileProviderAccessor)
    {
        _shapeTableManager = shapeTableManager;
        _extensionManager = extensionManager;
        _fileProviderAccessor = fileProviderAccessor;
    }

    /// <summary>
    /// Lists the template files that render the shapes of a theme. Each shape is listed once, with the file
    /// that renders it, e.g. a file of the theme rather than the one of a module that the theme overrides.
    /// </summary>
    /// <param name="themeId">The theme whose shape table is listed.</param>
    public async Task<IReadOnlyList<SourceTemplate>> GetTemplatesAsync(string themeId)
    {
        var shapeTable = await _shapeTableManager.GetShapeTableAsync(themeId);

        // The longest paths first, so that an extension nested in another one's folder is not taken for it.
        var extensions = _extensionManager.GetExtensions()
            .Where(extension => !string.IsNullOrEmpty(extension.SubPath))
            .OrderByDescending(extension => extension.SubPath.Length)
            .ToArray();

        var templates = new List<SourceTemplate>();

        foreach (var (bindingName, binding) in shapeTable.Bindings)
        {
            // Shapes bound to code have no file.
            if (string.IsNullOrEmpty(binding.BindingSource))
            {
                continue;
            }

            var filePath = binding.BindingSource.Replace('\\', '/');
            var extension = extensions.FirstOrDefault(extension =>
                filePath.StartsWith(extension.SubPath.TrimEnd('/') + '/', StringComparison.OrdinalIgnoreCase));

            if (extension == null)
            {
                continue;
            }

            var relativePath = filePath[(extension.SubPath.TrimEnd('/').Length + 1)..];

            if (!SourceTemplateNames.TryGetTemplateName(relativePath, out var templateName))
            {
                continue;
            }

            templates.Add(new SourceTemplate
            {
                // The shape table names are lower case; the name of the file keeps the case it is documented with,
                // unless another harvester bound the file to another shape.
                Name = string.Equals(templateName, bindingName, StringComparison.OrdinalIgnoreCase) ? templateName : bindingName,
                ExtensionId = extension.Id,
                RelativePath = relativePath,
                FilePath = filePath,
            });
        }

        return templates;
    }

    /// <summary>
    /// Reads the content of a template file.
    /// </summary>
    /// <returns>The content, or <see langword="null"/> when the file does not exist.</returns>
    public async Task<string> ReadAsync(SourceTemplate template)
    {
        var fileInfo = _fileProviderAccessor.FileProvider.GetFileInfo(template.FilePath);

        if (!fileInfo.Exists)
        {
            return null;
        }

        using var stream = fileInfo.CreateReadStream();
        using var reader = new StreamReader(stream);

        return await reader.ReadToEndAsync();
    }
}
