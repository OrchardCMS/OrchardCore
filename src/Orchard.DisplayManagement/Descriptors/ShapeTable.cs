using System.Collections.Generic;
using System.Linq;

namespace Orchard.DisplayManagement.Descriptors
{
    public class ShapeTable
    {
        public IDictionary<string, ShapeDescriptor> Descriptors { get; set; }
        public IDictionary<string, ShapeBinding> ShapeBindings { private get; set; }

        public ShapeTableBindings Bindings {
            get
            {
                return new ShapeTableBindings(this);
            }
        }

        public class ShapeTableBindings
        {
            private ShapeTable _shapeTable;

            public ShapeTableBindings(ShapeTable shapeTable)
            {
                _shapeTable = shapeTable;
            }

            public bool ContainsKey(string shapeAlternate)
            {
                return _shapeTable.ShapeBindings.ContainsKey(shapeAlternate);
            }

            public bool TryGetValue(string shapeAlternate, out ShapeBinding binding)
            {
                ShapeBinding shapeBinding;
                if (_shapeTable.ShapeBindings.TryGetValue(shapeAlternate, out shapeBinding))
                {
                    var index = shapeAlternate.IndexOf("__");
                    var shapeType = index < 0 ? shapeAlternate : shapeAlternate.Substring(0, index);

                    ShapeDescriptor descriptor;
                    if (_shapeTable.Descriptors.TryGetValue(shapeType, out descriptor))
                    {
                        binding = new ShapeBinding
                        {
                            ShapeDescriptor = descriptor,
                            BindingName = shapeBinding.BindingName,
                            BindingSource = shapeBinding.BindingSource,
                            BindingAsync = shapeBinding.BindingAsync
                        };

                        return true;
                    }
                }

                binding = null;
                return false;
            }
        }
    }
}