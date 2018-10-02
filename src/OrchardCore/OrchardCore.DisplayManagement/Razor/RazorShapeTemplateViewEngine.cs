using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc.ViewFeatures.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.DisplayManagement.Descriptors.ShapeTemplateStrategy;
using OrchardCore.DisplayManagement.Implementation;

namespace OrchardCore.DisplayManagement.Razor
{
    public class RazorShapeTemplateViewEngine : IShapeTemplateViewEngine
    {
        private readonly IOptions<MvcViewOptions> _viewEngine;
        private readonly List<string> _templateFileExtensions = new List<string>(new[] { RazorViewEngine.ViewExtension });

        public RazorShapeTemplateViewEngine(
            IOptions<MvcViewOptions> options,
            IEnumerable<IRazorViewExtensionProvider> viewExtensionProviders)
        {
            _viewEngine = options;
            _templateFileExtensions.AddRange(viewExtensionProviders.Select(x => x.ViewExtension));
        }

        public IEnumerable<string> TemplateFileExtensions
        {
            get
            {
                return _templateFileExtensions;
            }
        }

        public Task<IHtmlContent> RenderAsync(string relativePath, DisplayContext displayContext)
        {
            var viewName = "/" + relativePath;
            viewName = Path.ChangeExtension(viewName, RazorViewEngine.ViewExtension);

            if (displayContext.ViewContext.View != null)
            {
                var htmlHelper = MakeHtmlHelper(displayContext.ViewContext, displayContext.ViewContext.ViewData);
                return htmlHelper.PartialAsync(viewName, displayContext.Value);
            }
            else
            {
                // If the View is null, it means that the shape is being executed from a non-view origin / where no ViewContext was established by the view engine, but manually.
                // Manually creating a ViewContext works when working with Shape methods, but not when the shape is implemented as a Razor view template.
                // Horrible, but it will have to do for now.
                return RenderRazorViewAsync(viewName, displayContext);
            }
        }

        private async Task<IHtmlContent> RenderRazorViewAsync(string viewName, DisplayContext context)
        {
            var viewEngines = _viewEngine.Value.ViewEngines;

            if (viewEngines.Count == 0)
            {
                throw new InvalidOperationException(string.Format("'{0}.{1}' must not be empty. At least one '{2}' is required to locate a view for rendering.",
                    typeof(MvcViewOptions).FullName,
                    nameof(MvcViewOptions.ViewEngines),
                    typeof(IViewEngine).FullName));
            }

            for (var i = 0; i < viewEngines.Count; i++)
            {
                var viewEngineResult = viewEngines[0].FindView(context.ViewContext, viewName, isMainPage: false);
                if (viewEngineResult.Success)
                {
                    var bufferScope = context.ViewContext.HttpContext.RequestServices.GetRequiredService<IViewBufferScope>();
                    var viewBuffer = new ViewBuffer(bufferScope, viewEngineResult.ViewName, ViewBuffer.PartialViewPageSize);
                    using (var writer = new ViewBufferTextWriter(viewBuffer, context.ViewContext.Writer.Encoding))
                    {
                        // Forcing synchronous behavior so users don't have to await templates.
                        var view = viewEngineResult.View;
                        using (view as IDisposable)
                        {
                            var viewContext = new ViewContext(context.ViewContext, viewEngineResult.View, context.ViewContext.ViewData, writer);
                            await viewEngineResult.View.RenderAsync(viewContext);
                            return viewBuffer;
                        }
                    }
                }
            }

            return null;
        }

        private static IHtmlHelper MakeHtmlHelper(ViewContext viewContext, ViewDataDictionary viewData)
        {
            var newHelper = viewContext.HttpContext.RequestServices.GetRequiredService<IHtmlHelper>();

            var contextable = newHelper as IViewContextAware;
            if (contextable != null)
            {
                var newViewContext = new ViewContext(viewContext, viewContext.View, viewData, viewContext.Writer);
                contextable.Contextualize(newViewContext);
            }

            return newHelper;
        }
    }
}