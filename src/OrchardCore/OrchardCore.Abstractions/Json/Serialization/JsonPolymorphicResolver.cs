using System.Text.Json.Serialization.Metadata;

namespace System.Text.Json.Serialization;

public class JsonPolymorphicResolver<TDerived, TBase> : DefaultJsonTypeInfoResolver where TDerived : class where TBase : class
{
    public override JsonTypeInfo GetTypeInfo(Type type, JsonSerializerOptions options)
    {
        var jsonTypeInfo = base.GetTypeInfo(type, options);
       if (jsonTypeInfo.Type == typeof(TBase))
        {
            jsonTypeInfo.PolymorphismOptions = new JsonPolymorphismOptions
            {
                TypeDiscriminatorPropertyName = "$type",
                IgnoreUnrecognizedTypeDiscriminators = true,
                UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FailSerialization,
                DerivedTypes =
                {
                    new JsonDerivedType(typeof(TDerived), typeof(TDerived).FullName),
                }
            };
        }

        return jsonTypeInfo;
    }
}
