using Orchard.DependencyInjection;
using System.Collections.Generic;

namespace Orchard.DisplayManagement.Descriptors.ShapeTemplateStrategy
{
    public interface IShapeTemplateViewEngine : IDependency
    {
        IEnumerable<string> DetectTemplateFileNames(IEnumerable<string> fileNames);
    }
}