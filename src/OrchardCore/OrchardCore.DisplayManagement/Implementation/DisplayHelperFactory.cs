using System;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace OrchardCore.DisplayManagement.Implementation
{
    public class DisplayHelperFactory : IDisplayHelperFactory
    {
        private readonly IHtmlDisplay _displayManager;
        private readonly IShapeFactory _shapeFactory;
        private readonly IServiceProvider _serviceProvider;

        public DisplayHelperFactory(IHtmlDisplay displayManager, IShapeFactory shapeFactory, IServiceProvider serviceProvider)
        {
            _displayManager = displayManager;
            _shapeFactory = shapeFactory;
            _serviceProvider = serviceProvider;
        }

        public IDisplayHelper CreateHelper(ViewContext viewContext)
        {
            return new DisplayHelper(
                _displayManager,
                _shapeFactory,
                viewContext,
                _serviceProvider);
        }
    }
}