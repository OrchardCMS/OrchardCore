using Fluid;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Orchard.DisplayManagement;
using Orchard.DisplayManagement.Descriptors;
using Orchard.Liquid;
using Orchard.Templates.Models;

namespace Orchard.Templates.Services
{
    public class TemplatesShapeBindingResolver : IShapeBindingResolver
    {
        private TemplatesDocument _templatesDocument;
        private readonly ILiquidTemplateManager _liquidTemplateManager;
        private readonly IUrlHelperFactory _urlHelperFactory;

        public TemplatesShapeBindingResolver(
            TemplatesManager templatesManager, 
            ILiquidTemplateManager liquidTemplateManager,
            IUrlHelperFactory urlHelperFactory)
        {
            _templatesDocument = templatesManager.GetTemplatesDocumentAsync().GetAwaiter().GetResult();
            _liquidTemplateManager = liquidTemplateManager;
            _urlHelperFactory = urlHelperFactory;
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
                        
                        var actionContext = new ActionContext(displayContext.ViewContext.HttpContext, displayContext.ViewContext.RouteData, displayContext.ViewContext.ActionDescriptor);
                        var urlHelper = _urlHelperFactory.GetUrlHelper(actionContext);

                        context.LocalScope.SetValue("Context", displayContext.ViewContext);
                        context.LocalScope.SetValue("Model", displayContext.Value);
                        context.LocalScope.SetValue("ContentItem", ((dynamic)displayContext.Value).ContentItem);
                        context.AmbientValues.Add("UrlHelper", urlHelper);

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