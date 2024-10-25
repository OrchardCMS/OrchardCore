using System.Dynamic;
using System.Text.Json.Nodes;

#nullable enable

namespace System.Text.Json.Dynamic;

public abstract class JsonDynamicBase : DynamicObject
{
    public abstract JsonNode? Node { get; }

    public T? ToObject<T>() => Node.ToObject<T>();
}
