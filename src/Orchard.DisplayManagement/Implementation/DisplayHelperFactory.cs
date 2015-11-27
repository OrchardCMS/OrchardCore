using Microsoft.AspNet.Mvc.Rendering;

namespace Orchard.DisplayManagement.Implementation
{
    public class DisplayHelperFactory : IDisplayHelperFactory
    {
        private readonly IDisplayManager _displayManager;
        private readonly IShapeFactory _shapeFactory;

        public DisplayHelperFactory(IDisplayManager displayManager, IShapeFactory shapeFactory)
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