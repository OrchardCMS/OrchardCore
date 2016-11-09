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

            public bool ContainsKey(string shapeType)
            {
                var index = shapeType.IndexOf("__");
                var baseType = index < 0 ? shapeType : shapeType.Substring(0, index);

                ShapeDescriptor descriptor;
                if (_shapeTable.Descriptors.TryGetValue(baseType, out descriptor))
                {
                    return descriptor.Bindings.ContainsKey(shapeType);
                }

                return false;
            }

            public bool TryGetValue(string shapeType, out ShapeBinding binding)
            {
                var index = shapeType.IndexOf("__");
                var baseType = index < 0 ? shapeType : shapeType.Substring(0, index);

                ShapeDescriptor descriptor;
                if (_shapeTable.Descriptors.TryGetValue(baseType, out descriptor))
                {
                    return descriptor.Bindings.TryGetValue(shapeType, out binding);
                }

                binding = null;
                return false;
            }
        }
    }
}