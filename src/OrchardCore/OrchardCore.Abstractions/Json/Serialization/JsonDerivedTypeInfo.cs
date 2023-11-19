using System.Text.Json.Serialization.Metadata;

namespace System.Text.Json.Serialization;

public class JsonDerivedTypeInfo<TDerived, TBase> : IJsonDerivedTypeInfo
        where TDerived : class where TBase : class
{
    public JsonDerivedType DerivedType => new(
        typeof(TDerived),
        $"{typeof(TDerived).FullName}, {typeof(TDerived).Assembly.GetName().Name}");

    public Type BaseType => typeof(TBase);
}
