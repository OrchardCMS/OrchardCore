using System.Text.Encodings.Web;
using Fluid;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using OrchardCore.Admin;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.DisplayManagement.Liquid;
using OrchardCore.Liquid;
using OrchardCore.Templates.Models;

namespace OrchardCore.Templates.Services
{
    public class TemplatesShapeBindingResolver : IShapeBindingResolver
    {
        private readonly TemplatesDocument _templatesDocument;
        private readonly ILiquidTemplateManager _liquidTemplateManager;
        private readonly PreviewTemplatesProvider _previewTemplatesProvider;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TemplatesShapeBindingResolver(
            TemplatesManager templatesManager,
            ILiquidTemplateManager liquidTemplateManager,
            PreviewTemplatesProvider previewTemplatesProvider,
            IHttpContextAccessor httpContextAccessor)
        {
            _templatesDocument = templatesManager.GetTemplatesDocumentAsync().GetAwaiter().GetResult();
            _liquidTemplateManager = liquidTemplateManager;
            _previewTemplatesProvider = previewTemplatesProvider;
            _httpContextAccessor = httpContextAccessor;
        }

        public bool TryGetDescriptorBinding(string shapeType, out ShapeBinding shapeBinding)
        {
            if (AdminAttribute.IsApplied(_httpContextAccessor.HttpContext))
            {
                shapeBinding = null;
                return false;
            }

            var localTemplates = _previewTemplatesProvider.GetTemplates();

            if (localTemplates != null)
            {
                if (localTemplates.Templates.TryGetValue(shapeType, out var localTemplate))
                {
                    shapeBinding = BuildShapeBinding(shapeType, localTemplate);
                    return true;
                }
            }

            if (_templatesDocument.Templates.TryGetValue(shapeType, out var template))
            {
                shapeBinding = BuildShapeBinding(shapeType, template);

                return true;
            }
            else
            {
                shapeBinding = null;
                return false;
            }
        }

        private ShapeBinding BuildShapeBinding(string shapeType, Template template)
        {
            return new ShapeBinding()
            {
                ShapeDescriptor = new ShapeDescriptor() { ShapeType = shapeType },
                BindingName = shapeType,
                BindingSource = shapeType,
                BindingAsync = async displayContext =>
                {
                    var context = new TemplateContext();
                    await context.ContextualizeAsync(displayContext);
                    var htmlContent = await _liquidTemplateManager.RenderAsync(template.Content, HtmlEncoder.Default, context);
                    return new HtmlString(htmlContent);
                }
            };
        }
    }
}
