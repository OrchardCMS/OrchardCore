using Microsoft.Extensions.Logging;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.Modules;
using OrchardCore.Templates.Models;

namespace OrchardCore.Templates.Services;

/// <summary>
/// Renders a content item to list the shapes it renders, with their alternates and the template that renders each of them
/// with the active theme and the local templates, as the front end or the admin would.
/// </summary>
public sealed class TemplateInspectionService
{
    private const string AlternateSeparator = "__";

    private readonly ShapeInspector _shapeInspector;
    private readonly IContentItemDisplayManager _contentItemDisplayManager;
    private readonly IUpdateModelAccessor _updateModelAccessor;
    private readonly IShapeHtmlRenderer _shapeHtmlRenderer;
    private readonly IShapeTableManager _shapeTableManager;
    private readonly ShapeTemplateFiles _shapeTemplateFiles;
    private readonly ActiveThemes _activeThemes;
    private readonly TemplatesManager _templatesManager;
    private readonly AdminTemplatesManager _adminTemplatesManager;
    private readonly ILogger _logger;

    public TemplateInspectionService(
        ShapeInspector shapeInspector,
        IContentItemDisplayManager contentItemDisplayManager,
        IUpdateModelAccessor updateModelAccessor,
        IShapeHtmlRenderer shapeHtmlRenderer,
        IShapeTableManager shapeTableManager,
        ShapeTemplateFiles shapeTemplateFiles,
        ActiveThemes activeThemes,
        TemplatesManager templatesManager,
        AdminTemplatesManager adminTemplatesManager,
        ILogger<TemplateInspectionService> logger)
    {
        _shapeInspector = shapeInspector;
        _contentItemDisplayManager = contentItemDisplayManager;
        _updateModelAccessor = updateModelAccessor;
        _shapeHtmlRenderer = shapeHtmlRenderer;
        _shapeTableManager = shapeTableManager;
        _shapeTemplateFiles = shapeTemplateFiles;
        _activeThemes = activeThemes;
        _templatesManager = templatesManager;
        _adminTemplatesManager = adminTemplatesManager;
        _logger = logger;
    }

    /// <summary>
    /// Renders a content item and lists the shapes it renders.
    /// </summary>
    /// <param name="contentItem">The content item.</param>
    /// <param name="displayType">The display type to render it in, e.g. <c>Detail</c> or <c>Summary</c>.</param>
    /// <param name="adminTemplates">Whether the admin renders it, rather than the front end, which decides the theme and the local templates.</param>
    /// <returns>The shapes, each one before the shapes it renders, or an error when the content item could not be rendered.</returns>
    public async Task<(IReadOnlyList<InspectedShape> Shapes, string Error)> InspectAsync(ContentItem contentItem, string displayType, bool adminTemplates)
    {
        var shape = await _contentItemDisplayManager.BuildDisplayAsync(contentItem, _updateModelAccessor.ModelUpdater, displayType);

        IReadOnlyList<InspectedShape> shapes;
        string error = null;

        // The content item renders with the admin theme of this request: the shapes and their alternates are those of the
        // drivers, but the shapes a template of the site theme would render itself are not listed.
        _shapeInspector.Start();

        try
        {
            await _shapeHtmlRenderer.RenderAsync(shape);
        }
        catch (Exception ex) when (!ex.IsFatal())
        {
            _logger.LogWarning(ex, "The content item '{ContentItemId}' could not be rendered to inspect its templates.", contentItem.ContentItemId);
            error = ex.Message;
        }
        finally
        {
            shapes = _shapeInspector.Stop();
        }

        await ResolveTemplatesAsync(shapes, adminTemplates);

        return (shapes, error);
    }

    // Finds the template of each shape as the display manager does, with the theme and the local templates of the
    // front end, or the admin: the most specific alternate first, a local template winning over the theme and the features.
    private async Task ResolveTemplatesAsync(IReadOnlyList<InspectedShape> shapes, bool adminTemplates)
    {
        var themeId = await _activeThemes.GetThemeIdAsync(adminTemplates);
        var bindings = themeId == null ? null : (await _shapeTableManager.GetShapeTableAsync(themeId)).Bindings;
        var localTemplates = adminTemplates
            ? await _adminTemplatesManager.GetTemplatesDocumentAsync()
            : await _templatesManager.GetTemplatesDocumentAsync();

        foreach (var shape in shapes)
        {
            foreach (var name in GetCandidateNames(shape))
            {
                if (localTemplates.Templates.ContainsKey(name))
                {
                    shape.RenderedWith = name;
                    shape.IsLocal = true;
                    break;
                }

                if (bindings != null && bindings.TryGetValue(name, out var binding))
                {
                    shape.RenderedWith = name;

                    if (_shapeTemplateFiles.TryGetTemplateFile(binding.BindingSource, out var extensionId, out var relativePath))
                    {
                        shape.ExtensionId = extensionId;
                        shape.Source = relativePath;
                    }
                    else
                    {
                        shape.Source = binding.BindingSource;
                    }

                    break;
                }
            }
        }
    }

    // The alternates, the most specific first, then the type and its shorter forms, e.g. 'Content__Foo' then 'Content'.
    private static IEnumerable<string> GetCandidateNames(InspectedShape shape)
    {
        foreach (var alternate in shape.Alternates)
        {
            yield return alternate;
        }

        var type = shape.Type;

        while (!string.IsNullOrEmpty(type))
        {
            yield return type;

            var separator = type.LastIndexOf(AlternateSeparator, StringComparison.Ordinal);
            type = separator > 0 ? type[..separator] : null;
        }
    }
}
