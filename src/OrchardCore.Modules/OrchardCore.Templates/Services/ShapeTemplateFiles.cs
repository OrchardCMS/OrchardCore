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

    private readonly Dictionary<string, string[]> _viewFilePaths = [];

    private string[] _templateFileExtensions;
    private (IExtensionInfo Extension, string Prefix)[] _extensionsByPath;

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
    /// Gets the extensions whose template files render the shapes of a theme.
    /// </summary>
    /// <param name="themeId">The theme whose shape table is listed, if any.</param>
    /// <returns>The extension identifiers by template name, none when there is no theme.</returns>
    public async Task<IReadOnlyDictionary<string, string>> GetRenderingExtensionsAsync(string themeId)
        => themeId == null
            ? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            : (await GetTemplatesAsync(themeId))
                .GroupBy(template => template.Name, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(group => group.Key, group => group.First().ExtensionId, StringComparer.OrdinalIgnoreCase);

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
                foreach (var filePath in GetViewFilePaths($"{extensionPath}/{subPath}"))
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

    // Only the paths are kept for the request, not the templates, which the callers change.
    private string[] GetViewFilePaths(string folder)
    {
        if (!_viewFilePaths.TryGetValue(folder, out var filePaths))
        {
            _templateFileExtensions ??= _viewEngines.SelectMany(engine => engine.TemplateFileExtensions).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();

            filePaths = _fileProviderAccessor.FileProvider
                .GetViewFilePaths(folder, _templateFileExtensions, inViewsFolder: true, inDepth: false)
                .ToArray();

            _viewFilePaths[folder] = filePaths;
        }

        return filePaths;
    }

    // The longest paths first, so that an extension nested in another one's folder is not taken for it.
    private (IExtensionInfo Extension, string Prefix)[] GetExtensionsByPath()
        => _extensionsByPath ??= _extensionManager.GetExtensions()
            .Where(extension => !string.IsNullOrEmpty(extension.SubPath))
            .OrderByDescending(extension => extension.SubPath.Length)
            .Select(extension => (extension, extension.SubPath.TrimEnd('/') + '/'))
            .ToArray();

    private static bool TryGetTemplateFile(string bindingSource, (IExtensionInfo Extension, string Prefix)[] extensions, out IExtensionInfo extension, out string filePath, out string relativePath)
    {
        extension = null;
        filePath = null;
        relativePath = null;

        if (string.IsNullOrEmpty(bindingSource))
        {
            return false;
        }

        var path = bindingSource.Replace('\\', '/');
        var match = extensions.FirstOrDefault(item => path.StartsWith(item.Prefix, StringComparison.OrdinalIgnoreCase));

        if (match.Extension == null)
        {
            return false;
        }

        extension = match.Extension;
        filePath = path;
        relativePath = path[match.Prefix.Length..];

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
