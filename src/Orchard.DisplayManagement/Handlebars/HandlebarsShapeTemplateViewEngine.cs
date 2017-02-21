using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
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

        public Task<IHtmlContent> RenderAsync(string relativePath, DisplayContext displayContext)
        {
            var physicalPath = _hostingEnvironment.ContentRootFileProvider.GetFileInfo(relativePath).PhysicalPath;
            var render = _renderers.GetOrAdd(relativePath, Handlebars.Compile(File.ReadAllText(physicalPath)));

            dynamic context = new Composite();
            context.DisplayContext = displayContext;
            context.Model = displayContext.Value;

            return Task.FromResult<IHtmlContent>(new HtmlString(render(context)));
        }
    }
}