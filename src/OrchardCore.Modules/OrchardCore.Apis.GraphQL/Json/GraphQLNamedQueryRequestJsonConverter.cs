using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using GraphQL;

namespace OrchardCore.Apis.GraphQL.Json;

public class GraphQLNamedQueryRequestJsonConverter : JsonConverter<GraphQLNamedQueryRequest>
{
    public static readonly GraphQLNamedQueryRequestJsonConverter Instance = new();
    
    /// <summary>
    /// Name for the operation name parameter.
    /// See https://github.com/graphql/graphql-over-http/blob/master/spec/GraphQLOverHTTP.md#request-parameters
    /// </summary>
    private const string OperationNameKey = "operationName";

    /// <summary>
    /// Name for the query parameter.
    /// See https://github.com/graphql/graphql-over-http/blob/master/spec/GraphQLOverHTTP.md#request-parameters
    /// </summary>
    private const string QueryKey = "query";

    /// <summary>
    /// Name for the variables parameter.
    /// See https://github.com/graphql/graphql-over-http/blob/master/spec/GraphQLOverHTTP.md#request-parameters
    /// </summary>
    private const string VariablesKey = "variables";

    /// <summary>
    /// Name for the extensions parameter.
    /// See https://github.com/graphql/graphql-over-http/blob/master/spec/GraphQLOverHTTP.md#request-parameters
    /// </summary>
    private const string ExtensionsKey = "extensions";

    private const string NamedQueryKey = "namedQuery";


    public override void Write(Utf8JsonWriter writer, GraphQLNamedQueryRequest value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        if (value.Query != null)
        {
            writer.WritePropertyName(QueryKey);
            writer.WriteStringValue(value.Query);
        }
        if (value.OperationName != null)
        {
            writer.WritePropertyName(OperationNameKey);
            writer.WriteStringValue(value.OperationName);
        }
        if (value.Variables != null)
        {
            writer.WritePropertyName(VariablesKey);
            JsonSerializer.Serialize(writer, value.Variables, options);
        }
        if (value.Extensions != null)
        {
            writer.WritePropertyName(ExtensionsKey);
            JsonSerializer.Serialize(writer, value.Extensions, options);
        }
        if (value.NamedQuery != null)
        {
            writer.WritePropertyName(NamedQueryKey);
            JsonSerializer.Serialize(writer, value.NamedQuery, options);
        }
        writer.WriteEndObject();
    }

    public override GraphQLNamedQueryRequest Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
            throw new JsonException();

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

            string key = reader.GetString()!;

            //unexpected end of data
            if (!reader.Read())
                throw new JsonException();

            switch (key)
            {
                case QueryKey:
                    request.Query = reader.GetString()!;
                    break;
                case OperationNameKey:
                    request.OperationName = reader.GetString()!;
                    break;
                case NamedQueryKey:
                    request.NamedQuery = reader.GetString();
                    break;
                case VariablesKey:
                    request.Variables = JsonSerializer.Deserialize<Inputs>(ref reader, options);
                    break;
                case ExtensionsKey:
                    request.Extensions = JsonSerializer.Deserialize<Inputs>(ref reader, options);
                    break;
                default:
                    reader.Skip();
                    break;
            }
        }

        //unexpected end of data
        throw new JsonException();
    }
}
