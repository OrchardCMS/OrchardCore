using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Fluid;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Caching.Memory;
using Orchard.DisplayManagement;
using Orchard.DisplayManagement.Descriptors;
using Orchard.Environment.Cache;
using Orchard.Liquid;
using Orchard.Templates.Models;

namespace Orchard.Templates.Services
{
    public class TemplatesShapeBindingResolver : IShapeBindingResolver
    {
        private TemplatesDocument _templatesDocument;
        private readonly ILiquidTemplateManager _liquidTemplateManager;

        public TemplatesShapeBindingResolver(
            TemplatesManager templatesManager, 
            ILiquidTemplateManager liquidTemplateManager)
        {
            _templatesDocument = templatesManager.GetTemplatesDocumentAsync().GetAwaiter().GetResult();
            _liquidTemplateManager = liquidTemplateManager;
        }

        public bool TryGetDescriptorBinding(string shapeType, out ShapeBinding shapeBinding)
        {
            if (_templatesDocument.Templates.TryGetValue(shapeType, out var template))
            {
                shapeBinding = new ShapeBinding()
                {
                    BindingName = shapeType,
                    BindingSource = shapeType,
                    BindingAsync = async displayContext =>
                    {
                        var context = new TemplateContext();
                        context.LocalScope.SetValue("Context", displayContext.ViewContext);
                        context.LocalScope.SetValue("Model", displayContext.Value);
                        context.MemberAccessStrategy.Register(displayContext.Value.GetType());

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