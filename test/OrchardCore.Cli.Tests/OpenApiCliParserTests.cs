using System.Text.Json.Nodes;

namespace OrchardCore.Cli.Tests;

public class OpenApiCliParserTests
{
    [Fact]
    public void Parse_NullableTypesAndInheritedParameters_PreservesContract()
    {
        var operation = Assert.Single(OpenApiCliParser.Parse("""
            {"paths":{"/items/{id}":{
              "parameters":[{"name":"id","in":"path","required":true,"schema":{"type":"string"}},
                {"name":"take","in":"query","schema":{"type":"integer"}}],
              "put":{
                "parameters":[{"name":"take","in":"query","description":"override","schema":{"type":"integer"}}],
                "x-oc-cli":{"commandGroup":["items"],"verb":"update","inputMode":"options","arguments":[{"parameterName":"id","position":0}]},
                "requestBody":{"content":{"application/json":{"schema":{
                  "type":["object","null"],"properties":{
                    "roles":{"type":["null","array"],"items":{"type":"string"}},
                    "enabled":{"type":["boolean","null"]},
                    "count":{"type":["integer","string"]}
                  }
                }}}}
              }
            }}}
            """));
        Assert.Equal(2, operation.Parameters.Count);
        Assert.Equal(0, operation.Parameters.Single(parameter => parameter.Name == "id").ArgumentPosition);
        Assert.Equal("override", operation.Parameters.Single(parameter => parameter.Name == "take").Description);
        var roles = operation.RequestBodyProperties.Single(property => property.Name == "roles");
        Assert.Equal("array", roles.Type);
        Assert.Equal(["null", "array"], roles.AllowedTypes);
        CliApplication.ValidateDynamicJsonBody("""{"roles":[],"enabled":null,"count":"2"}""", operation);
        Assert.Throws<CliException>(() => CliApplication.ValidateDynamicJsonBody("""{"roles":"Editor"}""", operation));
    }

    [Fact]
    public void Parse_WhenCliMetadataExists_MapsCommandArgumentsAndRequestBody()
    {
        const string openApi = """
        {
          "paths": {
            "/api/items/{itemId}": {
              "delete": {
                "summary": "Delete an item",
                "parameters": [
                  {
                    "name": "itemId",
                    "in": "path",
                    "required": true,
                    "schema": { "type": "string" }
                  },
                  {
                    "name": "hardDelete",
                    "in": "query",
                    "schema": { "type": "boolean" }
                  }
                ],
                "requestBody": {
                  "required": true,
                  "content": {
                    "application/json": {
                      "schema": {
                        "type": "object",
                        "required": ["reason"],
                        "properties": {
                          "reason": {
                            "type": "string",
                            "enum": ["obsolete", "duplicate"],
                            "minLength": 3,
                            "maxLength": 20
                          },
                          "notify": { "type": "boolean" },
                          "metadata": {}
                        }
                      }
                    }
                  }
                },
                "x-oc-cli": {
                  "commandGroup": ["items"],
                  "verb": "delete",
                  "aliases": ["remove"],
                  "secretProperties": ["reason"],
                  "requiresConfirmation": true,
                  "defaultJsonBody": "{}",
                  "arguments": [
                    { "parameterName": "itemId", "position": 0 }
                  ],
                  "tableColumns": [
                    { "propertyPath": "id", "heading": "Id" }
                  ]
                }
              }
            }
          }
        }
        """;

        var operation = Assert.Single(OpenApiCliParser.Parse(openApi));

        Assert.Equal("DELETE", operation.Method);
        Assert.Equal("/api/items/{itemId}", operation.Path);
        Assert.Equal("items", Assert.Single(operation.CliMetadata.CommandGroup));
        Assert.Equal("delete", operation.CliMetadata.Verb);
        Assert.Equal("remove", Assert.Single(operation.CliMetadata.Aliases));
        Assert.Equal("reason", Assert.Single(operation.CliMetadata.SecretProperties));
        Assert.True(operation.CliMetadata.RequiresConfirmation);
        Assert.Equal("{}", operation.CliMetadata.DefaultJsonBody);
        Assert.Equal(0, Assert.Single(operation.CliMetadata.Arguments).Position);
        Assert.Equal("itemId", Assert.Single(operation.Parameters, static parameter => parameter.ArgumentPosition.HasValue).Name);
        var requiredProperty = Assert.Single(operation.RequestBodyProperties, static property => property.Required);
        Assert.Equal("reason", requiredProperty.Name);
        Assert.Equal(["\"obsolete\"", "\"duplicate\""], requiredProperty.AllowedValues);
        Assert.Equal(3, requiredProperty.MinimumLength);
        Assert.Equal(20, requiredProperty.MaximumLength);
        Assert.Null(Assert.Single(operation.RequestBodyProperties, static property => property.Name == "metadata").Type);
        Assert.True(operation.RequestBodyRequired);
        Assert.True(operation.HasJsonRequestBody);
        var requestSchema = Assert.IsType<JsonObject>(operation.RequestBodySchema);
        Assert.Equal("https://json-schema.org/draft/2020-12/schema", requestSchema["$schema"]?.GetValue<string>());
        Assert.Equal("object", requestSchema["type"]?.GetValue<string>());
        Assert.Equal("Id", Assert.Single(operation.CliMetadata.TableColumns).Heading);
    }

    [Fact]
    public void Parse_WhenRequestSchemaUsesComponents_CreatesStandaloneJsonSchema()
    {
        const string openApi = """
        {
          "jsonSchemaDialect": "https://json-schema.org/draft/2020-12/schema",
          "paths": {
            "/api/widgets": {
              "post": {
                "requestBody": {
                  "content": {
                    "application/json": {
                      "schema": { "$ref": "#/components/schemas/Widget" }
                    }
                  }
                },
                "x-oc-cli": {
                  "commandGroup": ["widgets"],
                  "verb": "create"
                }
              }
            }
          },
          "components": {
            "schemas": {
              "Widget": {
                "type": "object",
                "properties": {
                  "owner": { "$ref": "#/components/schemas/Owner" }
                }
              },
              "Owner": {
                "type": "object",
                "properties": {
                  "name": { "type": "string" }
                }
              },
              "Unused": {
                "type": "string"
              }
            }
          }
        }
        """;

        var operation = Assert.Single(OpenApiCliParser.Parse(openApi));
        var schema = Assert.IsType<JsonObject>(operation.RequestBodySchema);
        var ownerReference = schema["properties"]?["owner"]?["$ref"]?.GetValue<string>();
        var definitions = Assert.IsType<JsonObject>(schema["$defs"]);

        Assert.Equal("#/$defs/Owner", ownerReference);
        Assert.True(definitions.ContainsKey("Owner"));
        Assert.False(definitions.ContainsKey("Unused"));
    }
}
