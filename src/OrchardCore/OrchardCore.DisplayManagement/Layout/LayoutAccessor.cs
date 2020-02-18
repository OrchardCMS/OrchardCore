using System;
using System.Threading.Tasks;
using OrchardCore.DisplayManagement.Zones;

namespace OrchardCore.DisplayManagement.Layout
{
    public class LayoutAccessor : ILayoutAccessor
    {
        private IShape _layout;
        private readonly IShapeFactory _shapeFactory;

        public LayoutAccessor(IShapeFactory shapeFactory)
        {
            _shapeFactory = shapeFactory;
        }

        public async Task<IShape> GetLayoutAsync()
        {
            if (_layout == null)
            {
                // Create a shape whose properties are dynamically created as Zone shapes
                _layout = await _shapeFactory.CreateAsync("Layout", () => new ValueTask<IShape>(new ZoneHolding(() => _shapeFactory.CreateAsync("Zone"))));
            }

            if (_layout == null)
            {
                // At this point a Layout shape should always exist
                throw new ApplicationException("Fatal error, a Layout couldn't be created.");
            }

            return _layout;
        }
    }
}
