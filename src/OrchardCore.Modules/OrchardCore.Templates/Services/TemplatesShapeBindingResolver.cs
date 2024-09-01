using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Http;
using OrchardCore.Admin;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.Liquid;
using OrchardCore.Templates.Models;

namespace OrchardCore.Templates.Services;

public class TemplatesShapeBindingResolver : IShapeBindingResolver
{
    private TemplatesDocument _templatesDocument;
    private readonly TemplatesDocument _localTemplates;

    private readonly TemplatesManager _templatesManager;
    private readonly ILiquidTemplateManager _liquidTemplateManager;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly HtmlEncoder _htmlEncoder;
    private bool? _isAdmin;

    public TemplatesShapeBindingResolver(
        TemplatesManager templatesManager,
        ILiquidTemplateManager liquidTemplateManager,
        PreviewTemplatesProvider previewTemplatesProvider,
        IHttpContextAccessor httpContextAccessor,
        HtmlEncoder htmlEncoder)
    {
        _templatesManager = templatesManager;
        _liquidTemplateManager = liquidTemplateManager;
        _httpContextAccessor = httpContextAccessor;
        _htmlEncoder = htmlEncoder;
        _localTemplates = previewTemplatesProvider.GetTemplates();
    }

    public async Task<ShapeBinding> GetShapeBindingAsync(string shapeType)
    {
        // Cache this value since the service is scoped and this method is invoked for every
        // alternate of every shape.
        _isAdmin ??= AdminAttribute.IsApplied(_httpContextAccessor.HttpContext);

        if (_isAdmin.Value)
        {
            return null;
        }

        if (_localTemplates?.Templates?.TryGetValue(shapeType, out var localTemplate) == true)
        {
            return BuildShapeBinding(shapeType, localTemplate);
        }

        _templatesDocument ??= await _templatesManager.GetTemplatesDocumentAsync();

        if (_templatesDocument.Templates.TryGetValue(shapeType, out var template))
        {
            return BuildShapeBinding(shapeType, template);
        }

        return null;
    }

    private ShapeBinding BuildShapeBinding(string shapeType, Template template)
    {
        return new ShapeBinding()
        {
            BindingName = shapeType,
            BindingSource = shapeType,
            BindingAsync = displayContext => _liquidTemplateManager.RenderHtmlContentAsync(template.Content, _htmlEncoder, displayContext.Value)
        };
    }
}
