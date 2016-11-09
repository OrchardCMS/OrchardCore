using System.Collections.Generic;
using System.Linq;

namespace Orchard.DisplayManagement.Descriptors
{
    public class ShapeTable
    {
        public IDictionary<string, ShapeDescriptor> Descriptors { get; set; }
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
                var index = shapeAlternate.IndexOf("__");
                var shapeType = index < 0 ? shapeAlternate : shapeAlternate.Substring(0, index);

                ShapeDescriptor descriptor;
                if (_shapeTable.Descriptors.TryGetValue(shapeType, out descriptor))
                {
                    return descriptor.Bindings.ContainsKey(shapeAlternate);
                }

                return false;
            }

            public bool TryGetValue(string shapeAlternate, out ShapeBinding binding)
            {
                var index = shapeAlternate.IndexOf("__");
                var shapeType = index < 0 ? shapeAlternate : shapeAlternate.Substring(0, index);

                ShapeDescriptor descriptor;
                if (_shapeTable.Descriptors.TryGetValue(shapeType, out descriptor))
                {
                    return descriptor.Bindings.TryGetValue(shapeAlternate, out binding);
                }

                binding = null;
                return false;
            }
        }
    }
}