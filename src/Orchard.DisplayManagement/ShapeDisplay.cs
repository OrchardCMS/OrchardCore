using System.Collections.Generic;
using System.Linq;
using Orchard.DisplayManagement.Implementation;
using Orchard.DisplayManagement.Shapes;
using Microsoft.AspNet.Mvc.Rendering;
using Microsoft.AspNet.Http;

namespace Orchard.DisplayManagement
{
    public class ShapeDisplay : IShapeDisplay
    {
        private readonly IDisplayHelperFactory _displayHelperFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ShapeDisplay(
            IDisplayHelperFactory displayHelperFactory,
            IHttpContextAccessor httpContextAccessor)
        {
            _displayHelperFactory = displayHelperFactory;
            _httpContextAccessor = httpContextAccessor;
        }

        public string Display(Shape shape)
        {
            return Display((object)shape);
        }

        public string Display(object shape)
        {
            var viewContext = new ViewContext
            {
                HttpContext = _httpContextAccessor.HttpContext,
            };

            var display = _displayHelperFactory.CreateHelper(viewContext);

            return ((DisplayHelper)display).ShapeExecute(shape).ToString();
        }

        public IEnumerable<string> Display(IEnumerable<object> shapes)
        {
            return shapes.Select(Display).ToArray();
        }
    }
}