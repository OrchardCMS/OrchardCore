using System.Collections.Generic;

namespace OrchardCore.DisplayManagement.Descriptors
{
    public class ShapeTable
    {
        private readonly Dictionary<string, ShapeBinding> _shapeBindings;

        public ShapeTable(Dictionary<string, ShapeDescriptor> descriptors, Dictionary<string, ShapeBinding> bindings)
        {
            Descriptors = descriptors;
            _shapeBindings = bindings;
        }

        public Dictionary<string, ShapeDescriptor> Descriptors { get; }

        public ICollection<ShapeBinding> Bindings => _shapeBindings.Values;
        public ICollection<string> BindingNames => _shapeBindings.Keys;

        public bool TryGetShapeBinding(string shapeAlternate, string shapeType, out ShapeBinding binding)
        {
            ShapeBinding shapeBinding;

            if (_shapeBindings.TryGetValue(shapeAlternate, out shapeBinding))
            {
                binding = new ShapeBinding
                {
                    BindingName = shapeBinding.BindingName,
                    BindingSource = shapeBinding.BindingSource,
                    BindingAsync = shapeBinding.BindingAsync
                };

                return true;
            }

            binding = null;
            return false;
        }
    }
}
