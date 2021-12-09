using System.Collections.Generic;

namespace OrchardCore.DisplayManagement.Descriptors
{
    public class ShapeTable
    {
        public ShapeTable(Dictionary<string, ShapeDescriptor> descriptors, Dictionary<string, ShapeBinding> bindings)
        {
            Descriptors = descriptors;
            Bindings = bindings;
        }

        public Dictionary<string, ShapeDescriptor> Descriptors { get; }
        public Dictionary<string, ShapeBinding> Bindings { get; }
    }
}
