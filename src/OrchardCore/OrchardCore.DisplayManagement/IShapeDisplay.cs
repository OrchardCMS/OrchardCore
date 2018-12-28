using System.Collections.Generic;
using OrchardCore.DisplayManagement.Shapes;

namespace OrchardCore.DisplayManagement
{
    public interface IShapeDisplay
    {
        string Display(Shape shape);
        string Display(object shape);
        IEnumerable<string> Display(IEnumerable<object> shapes);
    }
}


