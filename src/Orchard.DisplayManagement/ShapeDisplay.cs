using System.Collections.Generic;
using Orchard.DisplayManagement.Implementation;
using Orchard.DisplayManagement.Shapes;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;

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
            var htmlContents = new List<IHtmlContent>();
            var htmlContentBuilder = new HtmlContentBuilder();

            foreach (var shape in shapes)
            {
                htmlContentBuilder.AppendHtml(await DisplayAsync(shape));
            }

            return htmlContentBuilder;
        }
    }
}