using System.Threading.Tasks;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Descriptors;

namespace OrchardCore.Testing.Stubs;

public class ShapeBindingResolverStub : IShapeBindingResolver
{
    private readonly ShapeBindingsDictionary _shapeBindings;

    public ShapeBindingResolverStub(ShapeBindingsDictionary shapeBindings)
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
