
namespace Orchard.DisplayManagement.Layout
{
    public class LayoutAccessor : ILayoutAccessor
    {
        private dynamic _layout;
        private readonly IShapeFactory _shapeFactory;

        public LayoutAccessor(IShapeFactory shapeFactory)
        {
            _shapeFactory = shapeFactory;
        }

        public dynamic GetLayout()
        {
            if(_layout == null)
            {
                _layout = _shapeFactory.Create("Layout", Arguments.Empty);
            }

            return _layout;
        }
    }
}
