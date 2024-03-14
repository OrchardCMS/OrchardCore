using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Descriptors;

namespace OrchardCore.Tests.Stubs
{
    public class TestShapeBindingsDictionary : Dictionary<string, ShapeBinding>
    {
        public TestShapeBindingsDictionary()
            : base(StringComparer.OrdinalIgnoreCase) { }
    }

    public class TestShapeBindingResolver : IShapeBindingResolver
    {
        private readonly TestShapeBindingsDictionary _shapeBindings;

        public TestShapeBindingResolver(TestShapeBindingsDictionary shapeBindings)
        {
            _shapeBindings = shapeBindings;
        }

        public Task<ShapeBinding> GetShapeBindingAsync(string shapeType)
        {
            if (_shapeBindings.TryGetValue(shapeType, out var binding))
            {
                return Task.FromResult(binding);
            }
            else
            {
                return Task.FromResult<ShapeBinding>(null);
            }
        }
    }
}
