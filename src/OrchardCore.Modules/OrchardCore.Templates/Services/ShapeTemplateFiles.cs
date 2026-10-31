using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.DisplayManagement.Descriptors.ShapeTemplateStrategy;
using OrchardCore.Environment.Extensions;
using OrchardCore.Mvc.FileProviders;
using OrchardCore.Templates.Models;

namespace OrchardCore.Templates.Services;

/// <summary>
/// Lists shape template files, from the shape table of a theme or from the files of extensions, so that a template
/// source only decides which of them it lists.
/// </summary>
public sealed class ShapeTemplateFiles
{
    private static readonly BasicShapeTemplateHarvester s_harvester = new();

    private readonly IShapeTableManager _shapeTableManager;
    private readonly IExtensionManager _extensionManager;
    private readonly IShapeTemplateFileProviderAccessor _fileProviderAccessor;
    private readonly IEnumerable<IShapeTemplateViewEngine> _viewEngines;

    public ShapeTemplateFiles(
        IShapeTableManager shapeTableManager,
        IExtensionManager extensionManager,
        IShapeTemplateFileProviderAccessor fileProviderAccessor,
        IEnumerable<IShapeTemplateViewEngine> viewEngines)
    {
        _shapeTableManager = shapeTableManager;
        _extensionManager = extensionManager;
        _fileProviderAccessor = fileProviderAccessor;
        _viewEngines = viewEngines;
    }

    /// <summary>
    /// Lists the template files that render the shapes of a theme. Each shape is listed once, with the file
    /// that renders it, e.g. a file of the theme rather than the one of a module that the theme overrides.
    /// </summary>
    /// <param name="themeId">The theme whose shape table is listed.</param>
    public async Task<IReadOnlyList<SourceTemplate>> GetTemplatesAsync(string themeId)
    {
        var shapeTable = await _shapeTableManager.GetShapeTableAsync(themeId);

        var extensions = GetExtensionsByPath();
        List<SourceTemplate> templates = [];

        foreach (var (bindingName, binding) in shapeTable.Bindings)
        {
            // Shapes bound to code have no file.
            if (!TryGetTemplateFile(binding.BindingSource, extensions, out var extension, out var filePath, out var relativePath) ||
                !SourceTemplateNames.TryGetTemplateName(relativePath, out var templateName))
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
    /// Finds the template file a shape binding renders, e.g. to tell which file renders a shape.
    /// </summary>
    /// <param name="bindingSource">The source of the binding, e.g. <c>Areas/TheBlogTheme/Views/Content.liquid</c>.</param>
    /// <param name="extensionId">The extension of the file.</param>
    /// <param name="relativePath">The path of the file inside its extension.</param>
    /// <returns>Whether the binding renders a file of an extension, rather than e.g. a shape defined in code.</returns>
    public bool TryGetTemplateFile(string bindingSource, out string extensionId, out string relativePath)
    {
        var found = TryGetTemplateFile(bindingSource, GetExtensionsByPath(), out var extension, out _, out relativePath);
        extensionId = extension?.Id;

        return found;
    }

    /// <summary>
    /// Lists the template files of extensions, whether they are enabled or not, e.g. a theme and the themes it is based on.
    /// When several extensions have a template for the same shape, the template of the first one is listed.
    /// </summary>
    /// <param name="extensionIds">The extensions, the one whose templates win first.</param>
    public IReadOnlyList<SourceTemplate> GetExtensionTemplates(IEnumerable<string> extensionIds)
    {
        var fileExtensions = _viewEngines.SelectMany(engine => engine.TemplateFileExtensions).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
        var templates = new Dictionary<string, SourceTemplate>(StringComparer.OrdinalIgnoreCase);

        foreach (var extensionId in extensionIds)
        {
            var extension = _extensionManager.GetExtension(extensionId);

            if (extension == null || !extension.Exists || string.IsNullOrEmpty(extension.SubPath))
            {
                continue;
            }

            var extensionPath = extension.SubPath.Replace('\\', '/').TrimEnd('/');

            foreach (var subPath in s_harvester.SubPaths())
            {
                var filePaths = _fileProviderAccessor.FileProvider.GetViewFilePaths(
                    $"{extensionPath}/{subPath}",
                    fileExtensions,
                    inViewsFolder: true,
                    inDepth: false);

                foreach (var filePath in filePaths)
                {
                    var relativePath = filePath[(extensionPath.Length + 1)..];

                    if (!SourceTemplateNames.TryGetTemplateName(relativePath, out var templateName))
                    {
                        continue;
                    }

                    templates.TryAdd(templateName, new SourceTemplate
                    {
                        Name = templateName,
                        ExtensionId = extension.Id,
                        RelativePath = relativePath,
                        FilePath = filePath,
                    });
                }
            }
        }

        return templates.Values.ToList();
    }

    // The longest paths first, so that an extension nested in another one's folder is not taken for it.
    private IExtensionInfo[] GetExtensionsByPath()
        => _extensionManager.GetExtensions()
            .Where(extension => !string.IsNullOrEmpty(extension.SubPath))
            .OrderByDescending(extension => extension.SubPath.Length)
            .ToArray();

    private static bool TryGetTemplateFile(string bindingSource, IExtensionInfo[] extensions, out IExtensionInfo extension, out string filePath, out string relativePath)
    {
        extension = null;
        filePath = null;
        relativePath = null;

        if (string.IsNullOrEmpty(bindingSource))
        {
            return false;
        }

        var path = bindingSource.Replace('\\', '/');
        extension = extensions.FirstOrDefault(extension => path.StartsWith(extension.SubPath.TrimEnd('/') + '/', StringComparison.OrdinalIgnoreCase));

        if (extension == null)
        {
            return false;
        }

        filePath = path;
        relativePath = path[(extension.SubPath.TrimEnd('/').Length + 1)..];

        return true;
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
