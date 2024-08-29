using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using OrchardCore.Json;
using OrchardCore.Json.Extensions;

namespace OrchardCore.Extensions;

public sealed class DocumentJsonSerializerOptionsConfiguration : IConfigureOptions<DocumentJsonSerializerOptions>
{
    private readonly JsonDerivedTypesOptions _derivedTypesOptions;

    public DocumentJsonSerializerOptionsConfiguration(IOptions<JsonDerivedTypesOptions> derivedTypesOptions)
    {
        _derivedTypesOptions = derivedTypesOptions.Value;
    }

    public void Configure(DocumentJsonSerializerOptions options)
    {
        options.SerializerOptions.Merge(JOptions.Default);
        options.SerializerOptions.TypeInfoResolverChain.Add(new PolymorphicJsonTypeInfoResolver(_derivedTypesOptions));
    }
}
