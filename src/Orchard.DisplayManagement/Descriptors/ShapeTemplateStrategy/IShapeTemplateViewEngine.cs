using System.Collections.Generic;

namespace Orchard.DisplayManagement.Descriptors.ShapeTemplateStrategy
{
    public interface IShapeTemplateViewEngine
    {
        IEnumerable<string> TemplateFileExtensions { get; }
    }
}