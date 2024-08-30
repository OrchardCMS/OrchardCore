using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using OrchardCore.Json;

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
        // Do not use the 'Merge' extension to avoid populating unwanted properties (e.g., Encoder, NumberHandling, TypeInfoResolver).
        options.SerializerOptions.DefaultIgnoreCondition = JOptions.Base.DefaultIgnoreCondition;
        options.SerializerOptions.ReferenceHandler = JOptions.Base.ReferenceHandler;
        options.SerializerOptions.ReadCommentHandling = JOptions.Base.ReadCommentHandling;
        options.SerializerOptions.PropertyNameCaseInsensitive = JOptions.Base.PropertyNameCaseInsensitive;
        options.SerializerOptions.AllowTrailingCommas = JOptions.Base.AllowTrailingCommas;
        options.SerializerOptions.WriteIndented = JOptions.Base.WriteIndented;
        options.SerializerOptions.PreferredObjectCreationHandling = JOptions.Base.PreferredObjectCreationHandling;

        options.SerializerOptions.TypeInfoResolverChain.Add(new PolymorphicJsonTypeInfoResolver(_derivedTypesOptions));

        foreach (var converter in JOptions.KnownConverters)
        {
            options.SerializerOptions.Converters.Add(converter);
        }
    }
}
