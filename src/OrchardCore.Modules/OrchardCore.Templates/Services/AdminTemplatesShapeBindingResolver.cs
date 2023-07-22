using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using OrchardCore.Admin;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.Liquid;
using OrchardCore.Templates.Models;

namespace OrchardCore.Templates.Services
{
    public class AdminTemplatesShapeBindingResolver : IShapeBindingResolver
    {
        private AdminTemplatesDocument _templatesDocument;
        private readonly AdminTemplatesManager _templatesManager;
        private readonly ILiquidTemplateManager _liquidTemplateManager;
        private readonly AdminPreviewTemplatesProvider _previewTemplatesProvider;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly HtmlEncoder _htmlEncoder;

        public AdminTemplatesShapeBindingResolver(
            AdminTemplatesManager templatesManager,
            ILiquidTemplateManager liquidTemplateManager,
            AdminPreviewTemplatesProvider previewTemplatesProvider,
            IHttpContextAccessor httpContextAccessor,
            HtmlEncoder htmlEncoder)
        {
            _templatesManager = templatesManager;
            _liquidTemplateManager = liquidTemplateManager;
            _previewTemplatesProvider = previewTemplatesProvider;
            _httpContextAccessor = httpContextAccessor;
            _htmlEncoder = htmlEncoder;
        }

        public async Task<ShapeBinding> GetShapeBindingAsync(string shapeType)
        {
            if (!AdminAttribute.IsApplied(_httpContextAccessor.HttpContext))
            {
                return null;
            }

            var localTemplates = _previewTemplatesProvider.GetTemplates();

            if (localTemplates != null)
            {
                if (localTemplates.Templates.TryGetValue(shapeType, out var localTemplate))
                {
                    return BuildShapeBinding(shapeType, localTemplate);
                }
            }

            _templatesDocument ??= await _templatesManager.GetTemplatesDocumentAsync();

            if (_templatesDocument.Templates.TryGetValue(shapeType, out var template))
            {
                return BuildShapeBinding(shapeType, template);
            }
            else
            {
                return null;
            }
        }

        private ShapeBinding BuildShapeBinding(string shapeType, Template template)
        {
            return new ShapeBinding()
            {
                BindingName = shapeType,
                BindingSource = shapeType,
                BindingAsync = async displayContext =>
                {
                    var content = await _liquidTemplateManager.RenderHtmlContentAsync(template.Content, _htmlEncoder, displayContext.Value);
                    return content;
                }
            };
        }
    }
}
