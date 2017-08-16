using Fluid;
using Microsoft.AspNetCore.Html;
using Orchard.Admin;
using Orchard.DisplayManagement;
using Orchard.DisplayManagement.Descriptors;
using Orchard.DisplayManagement.Fluid;
using Orchard.Liquid;
using Orchard.Templates.Models;
using Microsoft.AspNetCore.Http;

namespace Orchard.Templates.Services
{
    public class TemplatesShapeBindingResolver : IShapeBindingResolver
    {
        private TemplatesDocument _templatesDocument;
        private readonly ILiquidTemplateManager _liquidTemplateManager;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TemplatesShapeBindingResolver(
            TemplatesManager templatesManager, 
            ILiquidTemplateManager liquidTemplateManager,
            IHttpContextAccessor httpContextAccessor)
        {
            _templatesDocument = templatesManager.GetTemplatesDocumentAsync().GetAwaiter().GetResult();
            _liquidTemplateManager = liquidTemplateManager;
            _httpContextAccessor = httpContextAccessor;
        }

        public bool TryGetDescriptorBinding(string shapeType, out ShapeBinding shapeBinding)
        {
            if (AdminAttribute.IsApplied(_httpContextAccessor.HttpContext))
            {
                shapeBinding = null;
                return false;
            }

            if (_templatesDocument.Templates.TryGetValue(shapeType, out var template))
            {
                shapeBinding = new ShapeBinding()
                {
                    ShapeDescriptor = new ShapeDescriptor() { ShapeType = shapeType },
                    BindingName = shapeType,
                    BindingSource = shapeType,
                    BindingAsync = async displayContext =>
                    {
                        var context = new TemplateContext();
                        context.Contextualize(displayContext);
                        var htmlContent = await _liquidTemplateManager.RenderAsync(template.Content, context);
                        return new HtmlString(htmlContent);
                    }
                };

                return true;
            }
            else
            {
                shapeBinding = null;
                return false;
            }
        }
    }
}