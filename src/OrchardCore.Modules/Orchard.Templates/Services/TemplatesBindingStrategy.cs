using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.FileSystemGlobbing;
using Microsoft.Extensions.FileSystemGlobbing.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Orchard.DisplayManagement.Descriptors;
using Orchard.DisplayManagement.Descriptors.ShapeTemplateStrategy;
using Orchard.Environment.Extensions;
using Orchard.Environment.Extensions.Features;
using Orchard.Environment.Shell;

namespace Orchard.Templates.Services
{
    public class TemplatesBindingStrategy : IShapeTableHarvester
    {
        private readonly IExtensionManager _extensionManager;

        public TemplatesBindingStrategy(IExtensionManager extensionManager)
        {
            _extensionManager = extensionManager;
        }

        public void Discover(ShapeTableBuilder builder)
        {
            var feature = _extensionManager.GetFeatures().Where(f => f.Id == "TheBlogTheme").FirstOrDefault();

            builder.Describe("Content_Detail__Page")
                .From(feature)
                .BoundAs(
                    "Templates/Content_Page", shapeDescriptor => displayContext =>
                        Task.FromResult<IHtmlContent>(new HtmlString("Boo!")));
        }
    }
}