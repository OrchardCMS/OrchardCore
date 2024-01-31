using System.Text.Json.Serialization.Metadata;
using Cysharp.Text;

namespace System.Text.Json.Serialization;

public class JsonDerivedTypeInfo<TDerived, TBase> : IJsonDerivedTypeInfo
        where TDerived : class where TBase : class
{
    public JsonDerivedType DerivedType => new(typeof(TDerived), CreateTypeDiscriminator<TDerived>());

    public Type BaseType => typeof(TBase);

    internal static string CreateTypeDiscriminator<T>()
    {
        string fullyQualifiedTypeName = typeof(T).AssemblyQualifiedName;
        fullyQualifiedTypeName = RemoveAssemblyDetails(fullyQualifiedTypeName);
        return fullyQualifiedTypeName;
    }

    private static string RemoveAssemblyDetails(string fullyQualifiedTypeName)
    {
        using var builder = ZString.CreateStringBuilder();

        // loop through the type name and filter out qualified assembly details from nested type names
        bool writingAssemblyName = false;
        bool skippingAssemblyDetails = false;
        bool followBrackets = false;
        for (int i = 0; i < fullyQualifiedTypeName.Length; i++)
        {
            char current = fullyQualifiedTypeName[i];
            switch (current)
            {
                case '[':
                    writingAssemblyName = false;
                    skippingAssemblyDetails = false;
                    followBrackets = true;
                    builder.Append(current);
                    break;
                case ']':
                    writingAssemblyName = false;
                    skippingAssemblyDetails = false;
                    followBrackets = false;
                    builder.Append(current);
                    break;
                case ',':
                    if (followBrackets)
                    {
                        builder.Append(current);
                    }
                    else if (!writingAssemblyName)
                    {
                        writingAssemblyName = true;
                        builder.Append(current);
                    }
                    else
                    {
                        skippingAssemblyDetails = true;
                    }
                    break;
                default:
                    followBrackets = false;
                    if (!skippingAssemblyDetails)
                    {
                        builder.Append(current);
                    }
                    break;
            }
        }

        return builder.ToString();
    }
}
