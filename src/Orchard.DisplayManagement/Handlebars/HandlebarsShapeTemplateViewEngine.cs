using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using Orchard.DisplayManagement.Descriptors;
using Orchard.DisplayManagement.Descriptors.ShapeTemplateStrategy;
using Orchard.DisplayManagement.Implementation;

namespace Orchard.DisplayManagement.Handlebars
{
    public class HandlebarsShapeTemplateViewEngine : IHandlebarsShapeTemplateViewEngine
    {
        private static ConcurrentDictionary<string, Func<object, string>> _renderers = new ConcurrentDictionary<string, Func<object, string>>();

        public IEnumerable<string> TemplateFileExtensions
        {
            get {
                return new[] { "hbs" };
            }
        }

        public Task<IHtmlContent> RenderAsync(ShapeDescriptor shapeDescriptor, DisplayContext displayContext, HarvestShapeInfo harvestShapeInfo)
        {
            var render = _renderers.GetOrAdd(harvestShapeInfo.TemplateVirtualPath,
                HandlebarsDotNet.Handlebars.Compile(File.ReadAllText(harvestShapeInfo.PhysicalPath)));

            return Task.FromResult<IHtmlContent>(new HtmlString(render(displayContext.Value)));
        }
    }
}