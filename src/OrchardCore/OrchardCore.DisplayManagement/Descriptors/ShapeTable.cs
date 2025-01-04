namespace OrchardCore.DisplayManagement.Descriptors;

public class ShapeTable
{
    public ShapeTable(IDictionary<string, ShapeDescriptor> descriptors, IDictionary<string, ShapeBinding> bindings)
    {
        Descriptors = descriptors;
        Bindings = bindings;
    }

    public IDictionary<string, ShapeDescriptor> Descriptors { get; }
    public IDictionary<string, ShapeBinding> Bindings { get; }
}
