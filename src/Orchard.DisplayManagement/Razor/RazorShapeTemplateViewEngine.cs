using Orchard.DisplayManagement.Descriptors.ShapeTemplateStrategy;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Orchard.DisplayManagement.Razor
{
    public class RazorShapeTemplateViewEngine : IShapeTemplateViewEngine
    {
        public IEnumerable<string> DetectTemplateFileNames(IEnumerable<string> fileNames)
        {
            return fileNames.Where(fileName => fileName.EndsWith(".cshtml", StringComparison.OrdinalIgnoreCase));
        }
    }
}