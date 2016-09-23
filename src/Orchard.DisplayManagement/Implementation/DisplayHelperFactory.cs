using Microsoft.AspNetCore.Mvc.Rendering;

namespace Orchard.DisplayManagement.Implementation
{
    public class DisplayHelperFactory : IDisplayHelperFactory
    {
        private readonly IHtmlDisplay _displayManager;
        private readonly IShapeFactory _shapeFactory;

        public DisplayHelperFactory(IHtmlDisplay displayManager, IShapeFactory shapeFactory)
        {
            _displayManager = displayManager;
            _shapeFactory = shapeFactory;
        }

        public dynamic CreateHelper(ViewContext viewContext)
        {
            return new DisplayHelper(
                _displayManager,
                _shapeFactory,
                viewContext);
        }
    }
}