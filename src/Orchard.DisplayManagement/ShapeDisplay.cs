using System.Collections.Generic;
using Orchard.DisplayManagement.Implementation;
using Orchard.DisplayManagement.Shapes;
using Microsoft.AspNet.Mvc.Rendering;
using Microsoft.AspNet.Http;
using System.Threading.Tasks;
using Microsoft.AspNet.Html;

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

        public async Task<IHtmlContent> DisplayAsync(object shape)
        {
            var viewContext = new ViewContext
            {
                HttpContext = _httpContextAccessor.HttpContext,
            };

            var display = _displayHelperFactory.CreateHelper(viewContext);

            return (await ((DisplayHelper)display).ShapeExecuteAsync(shape));
        }

        public async Task<IHtmlContent> DisplayAsync(IEnumerable<object> shapes)
        {
            var result = new List<IHtmlContent>();
            foreach (var shape in shapes)
            {
                result.Add(await DisplayAsync(shape));
            }

            return new DisplayHelper.Combined(result);
        }
    }
}