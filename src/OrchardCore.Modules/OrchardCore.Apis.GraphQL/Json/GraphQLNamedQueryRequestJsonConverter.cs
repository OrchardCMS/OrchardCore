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
    private const string OPERATION_NAME_KEY = "operationName";

    /// <summary>
    /// Name for the query parameter.
    /// See https://github.com/graphql/graphql-over-http/blob/master/spec/GraphQLOverHTTP.md#request-parameters
    /// </summary>
    private const string QUERY_KEY = "query";

    /// <summary>
    /// Name for the variables parameter.
    /// See https://github.com/graphql/graphql-over-http/blob/master/spec/GraphQLOverHTTP.md#request-parameters
    /// </summary>
    private const string VARIABLES_KEY = "variables";

    /// <summary>
    /// Name for the extensions parameter.
    /// See https://github.com/graphql/graphql-over-http/blob/master/spec/GraphQLOverHTTP.md#request-parameters
    /// </summary>
    private const string EXTENSIONS_KEY = "extensions";

    private const string NAMEDQUERY_KEY = "namedQuery";


    public override void Write(Utf8JsonWriter writer, GraphQLNamedQueryRequest value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        if (value.Query != null)
        {
            writer.WritePropertyName(QUERY_KEY);
            writer.WriteStringValue(value.Query);
        }
        if (value.OperationName != null)
        {
            writer.WritePropertyName(OPERATION_NAME_KEY);
            writer.WriteStringValue(value.OperationName);
        }
        if (value.Variables != null)
        {
            writer.WritePropertyName(VARIABLES_KEY);
            JsonSerializer.Serialize(writer, value.Variables, options);
        }
        if (value.Extensions != null)
        {
            writer.WritePropertyName(EXTENSIONS_KEY);
            JsonSerializer.Serialize(writer, value.Extensions, options);
        }
        if (value.NamedQuery != null)
        {
            writer.WritePropertyName(NAMEDQUERY_KEY);
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
                return request;

            if (reader.TokenType != JsonTokenType.PropertyName)
                throw new JsonException();

            string key = reader.GetString()!;

            //unexpected end of data
            if (!reader.Read())
                throw new JsonException();

            switch (key)
            {
                case QUERY_KEY:
                    request.Query = reader.GetString()!;
                    break;
                case OPERATION_NAME_KEY:
                    request.OperationName = reader.GetString()!;
                    break;
                case NAMEDQUERY_KEY:
                    request.NamedQuery = reader.GetString();
                    break;
                case VARIABLES_KEY:
                    request.Variables = JsonSerializer.Deserialize<Inputs>(ref reader, options);
                    break;
                case EXTENSIONS_KEY:
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
