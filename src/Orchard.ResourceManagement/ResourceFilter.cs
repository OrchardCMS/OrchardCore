using System.Web.Mvc;
using Orchard.DisplayManagement;
using Orchard.Mvc.Filters;

namespace Orchard.ResourceManagement {
    public class ResourceFilter : FilterProvider, IResultFilter {
        private readonly IWorkContextAccessor _workContextAccessor;
        private readonly dynamic _shapeFactory;

        public ResourceFilter(
            IWorkContextAccessor workContextAccessor, 
            IShapeFactory shapeFactory) {
            _workContextAccessor = workContextAccessor;
            _shapeFactory = shapeFactory;
        }

        public void OnResultExecuting(ResultExecutingContext filterContext) {
            // should only run on a full view rendering result
            if (!(filterContext.Result is ViewResult))
                return;

            var ctx = _workContextAccessor.GetContext();
            var head = ctx.Layout.Head;
            var tail = ctx.Layout.Tail;
            head.Add(_shapeFactory.Metas());
            head.Add(_shapeFactory.HeadLinks());
            head.Add(_shapeFactory.StylesheetLinks());
            head.Add(_shapeFactory.HeadScripts());
            tail.Add(_shapeFactory.FootScripts());
        }

        public void OnResultExecuted(ResultExecutedContext filterContext) {
        }
    }
}