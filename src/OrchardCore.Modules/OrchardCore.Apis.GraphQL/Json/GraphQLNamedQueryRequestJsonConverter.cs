using System.Text.Json;
using System.Text.Json.Serialization;
using GraphQL;

namespace OrchardCore.Apis.GraphQL.Json;

public class GraphQLNamedQueryRequestJsonConverter : JsonConverter<GraphQLNamedQueryRequest>
{
    public static readonly GraphQLNamedQueryRequestJsonConverter Instance = new();

    /// <summary>
    /// Name for the operation name parameter.
    /// See https://github.com/graphql/graphql-over-http/blob/master/spec/GraphQLOverHTTP.md#request-parameters.
    /// </summary>
    private const string _operationNameKey = "operationName";

    /// <summary>
    /// Name for the query parameter.
    /// See https://github.com/graphql/graphql-over-http/blob/master/spec/GraphQLOverHTTP.md#request-parameters.
    /// </summary>
    private const string _queryKey = "query";

    /// <summary>
    /// Name for the variables parameter.
    /// See https://github.com/graphql/graphql-over-http/blob/master/spec/GraphQLOverHTTP.md#request-parameters.
    /// </summary>
    private const string _variablesKey = "variables";

    /// <summary>
    /// Name for the extensions parameter.
    /// See https://github.com/graphql/graphql-over-http/blob/master/spec/GraphQLOverHTTP.md#request-parameters.
    /// </summary>
    private const string _extensionsKey = "extensions";

    /// <summary>
    /// Name for the namedQuery parameter.
    /// </summary>
    private const string _namedQueryKey = "namedQuery";

    public override void Write(Utf8JsonWriter writer, GraphQLNamedQueryRequest value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        if (value.Query != null)
        {
            writer.WritePropertyName(_queryKey);
            writer.WriteStringValue(value.Query);
        }

        if (value.OperationName != null)
        {
            writer.WritePropertyName(_operationNameKey);
            writer.WriteStringValue(value.OperationName);
        }

        if (value.Variables != null)
        {
            writer.WritePropertyName(_variablesKey);
            JsonSerializer.Serialize(writer, value.Variables, options);
        }

        if (value.Extensions != null)
        {
            writer.WritePropertyName(_extensionsKey);
            JsonSerializer.Serialize(writer, value.Extensions, options);
        }

        if (value.NamedQuery != null)
        {
            writer.WritePropertyName(_namedQueryKey);
            JsonSerializer.Serialize(writer, value.NamedQuery, options);
        }

        writer.WriteEndObject();
    }

    public override GraphQLNamedQueryRequest Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException();
        }

        var request = new GraphQLNamedQueryRequest();

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                return request;
            }

            if (reader.TokenType != JsonTokenType.PropertyName)
            {
                throw new JsonException();
            }

            var key = reader.GetString()!;

            // Unexpected end of data.
            if (!reader.Read())
            {
                throw new JsonException();
            }

            switch (key)
            {
                case _queryKey:
                    request.Query = reader.GetString()!;
                    break;
                case _operationNameKey:
                    request.OperationName = reader.GetString()!;
                    break;
                case _namedQueryKey:
                    request.NamedQuery = reader.GetString();
                    break;
                case _variablesKey:
                    request.Variables = JsonSerializer.Deserialize<Inputs>(ref reader, options);
                    break;
                case _extensionsKey:
                    request.Extensions = JsonSerializer.Deserialize<Inputs>(ref reader, options);
                    break;
                default:
                    reader.Skip();
                    break;
            }
        }

        // Unexpected end of data.
        throw new JsonException();
    }
}
