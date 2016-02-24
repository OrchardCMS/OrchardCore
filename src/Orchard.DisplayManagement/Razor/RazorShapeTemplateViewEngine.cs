using Orchard.DisplayManagement.Descriptors.ShapeTemplateStrategy;
using System.Collections.Generic;

namespace Orchard.DisplayManagement.Razor
{
    public class RazorShapeTemplateViewEngine : IShapeTemplateViewEngine
    {
        public IEnumerable<string> TemplateFileExtensions
        {
            get {
                return new[] { "cshtml" };
            }
        }
    }
}