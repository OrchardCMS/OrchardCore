using System.Text.Json.Serialization.Metadata;

namespace System.Text.Json.Serialization;

public interface IJsonDerivedTypeInfo
{
    JsonDerivedType DerivedType { get; }

    Type BaseType { get; }
}
