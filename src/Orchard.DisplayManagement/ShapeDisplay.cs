using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Orchard.DisplayManagement.Implementation;
using Orchard.DisplayManagement.Shapes;

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

        public Task<IHtmlContent> DisplayAsync(Shape shape)
        {
            return DisplayAsync((object)shape);
        }

        public Task<IHtmlContent> DisplayAsync(object shape)
        {
            var viewContext = new ViewContext
            {
                HttpContext = _httpContextAccessor.HttpContext,
            };

            var display = _displayHelperFactory.CreateHelper(viewContext);

            return ((DisplayHelper)display).ShapeExecuteAsync(shape);
        }

        public Task<IHtmlContent> DisplayAsync(IEnumerable<object> shapes)
        {
            var viewContext = new ViewContext
            {
                HttpContext = _httpContextAccessor.HttpContext,
            };

            var display = _displayHelperFactory.CreateHelper(viewContext);

            return ((DisplayHelper)display).ShapeExecuteAsync(shapes);
        }
    }
}