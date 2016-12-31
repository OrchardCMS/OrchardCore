using System;
using Glimpse.Common;
using Glimpse.Initialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Orchard.DisplayManagement;
using Orchard.DisplayManagement.Layout;

namespace Orchard.Glimpse
{
    public class GlimpseFilter : IResultFilter
    {
        private readonly ILayoutAccessor _layoutAccessor;
        private readonly IShapeFactory _shapeFactory;
        private readonly Guid _requestId;
        private readonly ResourceOptions _resourceOptions;

        public GlimpseFilter(
            ILayoutAccessor layoutAccessor,
            IShapeFactory shapeFactory,
            IGlimpseContextAccessor context, 
            IResourceOptionsProvider resourceOptionsProvider)
        {
            _layoutAccessor = layoutAccessor;
            _shapeFactory = shapeFactory;
            _requestId = context.RequestId;
            _resourceOptions = resourceOptionsProvider.BuildInstance();
        }

        public void OnResultExecuting(ResultExecutingContext filterContext)
        {
            if (!(filterContext.Result is ViewResult))
            {
                return;
            }

            // Populate main nav
            IShape glimpseShape = _shapeFactory.Create("Glimpse",
                Arguments.From(new
                {
                    RequestId = _requestId,
                    ResourceOptions = _resourceOptions
                }));

            _layoutAccessor.GetLayout().Footer.Add(glimpseShape);
        }

        public void OnResultExecuted(ResultExecutedContext filterContext)
        {
        }
    }
}