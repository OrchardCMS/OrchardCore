using System.Collections.Generic;
using Orchard.DisplayManagement.Shapes;
using Orchard.DependencyInjection;

namespace Orchard.DisplayManagement
{
    public interface IShapeDisplay : IDependency
    {
        string Display(Shape shape);
        string Display(object shape);
        IEnumerable<string> Display(IEnumerable<object> shapes);
    }
}