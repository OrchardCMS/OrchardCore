using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using OrchardCore.DisplayManagement.Implementation;
using OrchardCore.DisplayManagement.Shapes;

namespace OrchardCore.DisplayManagement
{
    public class ShapeDisplay : IShapeDisplay
    {
        private readonly IDisplayHelperFactory _displayHelperFactory;        
        private readonly HttpContext _httpContextBase;
     
        public ShapeDisplay(
            IDisplayHelperFactory displayHelperFactory,           
            HttpContext httpContextBase)
        {
            _displayHelperFactory = displayHelperFactory;           
            _httpContextBase = httpContextBase;           
        }

        public string Display(Shape shape)
        {
            return Display((object)shape);
        }

        public string Display(object shape)
        {
            var viewContext = new ViewContext
            {
                HttpContext = _httpContextBase                
            };          
            var display = _displayHelperFactory.CreateHelper(viewContext);
            return ((DisplayHelper)display).ShapeExecuteAsync(shape).ToString();
        }

        public IEnumerable<string> Display(IEnumerable<object> shapes)
        {
            return shapes.Select(Display).ToArray();
        }        
    }
}

