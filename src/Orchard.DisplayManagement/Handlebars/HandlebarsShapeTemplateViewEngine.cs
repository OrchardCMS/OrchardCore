using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using HandlebarsDotNet;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;
using Orchard.DisplayManagement.Descriptors.ShapeTemplateStrategy;
using Orchard.DisplayManagement.Implementation;
using Orchard.DisplayManagement.Shapes;

namespace Orchard.DisplayManagement.HandleBars
{
    public class HandleBarsShapeTemplateViewEngine : IShapeTemplateViewEngine
    {
        private readonly ConcurrentDictionary<string, Func<object, string>> _renderers;
        private readonly IHostingEnvironment _hostingEnvironment;

        public HandleBarsShapeTemplateViewEngine(
            IHostingEnvironment hostingEnvironment)
        {
            _renderers = new ConcurrentDictionary<string, Func<object, string>>();
            _hostingEnvironment = hostingEnvironment;
        }

        public IEnumerable<string> TemplateFileExtensions
        {
            get {
                return new[] { ".hbs" };
            }
        }

        public async Task<IHtmlContent> RenderAsync(string relativePath, DisplayContext displayContext)
        {
            var viewEngineInstance = displayContext.ServiceProvider
                .GetRequiredService<IEnumerable<IShapeTemplateViewEngine>>()
                .FirstOrDefault(e => e is HandleBarsShapeTemplateViewEngine);

            return await ((HandleBarsShapeTemplateViewEngine)viewEngineInstance)
                .RenderAsyncInternal(relativePath, displayContext);
        }

        private Task<IHtmlContent> RenderAsyncInternal(string relativePath, DisplayContext displayContext)
        {
            var physicalPath = _hostingEnvironment.ContentRootFileProvider.GetFileInfo(relativePath).PhysicalPath;
            var render = _renderers.GetOrAdd(relativePath, Handlebars.Compile(File.ReadAllText(physicalPath)));

            var urlHelperFactory = displayContext.ServiceProvider.GetService<IUrlHelperFactory>();
            var urlHelper = urlHelperFactory.GetUrlHelper(displayContext.ViewContext);

            dynamic context = new Composite();
            context.DisplayContext = displayContext;
            context.Model = displayContext.Value;

            if (displayContext.ViewContext.View != null)
            {
                context.Html = MakeHtmlHelper(displayContext.ViewContext, displayContext.ViewContext.ViewData);
            }

            context.Url = urlHelper;

            return Task.FromResult<IHtmlContent>(new HtmlString(render(context)));
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