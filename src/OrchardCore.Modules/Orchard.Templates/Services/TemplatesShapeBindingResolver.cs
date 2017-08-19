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
        private readonly PreviewTemplatesProvider _previewTemplatesProvider;

        public TemplatesShapeBindingResolver(
            TemplatesManager templatesManager, 
            ILiquidTemplateManager liquidTemplateManager,
            IUrlHelperFactory urlHelperFactory,
            PreviewTemplatesProvider previewTemplatesProvider)
        {
            _templatesDocument = templatesManager.GetTemplatesDocumentAsync().GetAwaiter().GetResult();
            _liquidTemplateManager = liquidTemplateManager;
            _urlHelperFactory = urlHelperFactory;
            _previewTemplatesProvider = previewTemplatesProvider;
        }

        public bool TryGetDescriptorBinding(string shapeType, out ShapeBinding shapeBinding)
        {
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

                    var actionContext = new ActionContext(displayContext.ViewContext.HttpContext, displayContext.ViewContext.RouteData, displayContext.ViewContext.ActionDescriptor);
                    var urlHelper = _urlHelperFactory.GetUrlHelper(actionContext);

                    context.LocalScope.SetValue("Context", displayContext.ViewContext);
                    context.AmbientValues.Add("UrlHelper", urlHelper);

                    context.LocalScope.SetValue("Model", displayContext.Value);
                    context.MemberAccessStrategy.Register(displayContext.Value.GetType());

                    var htmlContent = await _liquidTemplateManager.RenderAsync(template.Content, context);
                    return new HtmlString(htmlContent);
                }
            };
        }
    }
}