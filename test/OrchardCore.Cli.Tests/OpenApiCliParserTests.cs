using System.Text.Json.Nodes;

namespace OrchardCore.Cli.Tests;

public class OpenApiCliParserTests
{
    [Theory]
    [InlineData("""{"type":"object","properties":{"apiKey":{"type":"string","writeOnly":true},"name":{"type":"string"}}}""", "apiKey", true)]
    [InlineData("""{"type":"object","properties":{"apiKey":{"$ref":"#/components/schemas/Secret"},"name":{"type":"string"}}}""", "apiKey", true)]
    [InlineData("""{"type":"object","properties":{"apiKey":{"$ref":"#/components/schemas/Plain","writeOnly":true},"name":{"type":"string"}}}""", "apiKey", true)]
    [InlineData("""{"type":"object","properties":{"nested":{"type":"object","properties":{"apiKey":{"type":"string","writeOnly":true}}},"name":{"type":"string"}}}""", "nested", true)]
    [InlineData("""{"type":"object","properties":{"nested":{"anyOf":[{"$ref":"#/components/schemas/Holder"},{"type":"null"}]},"name":{"type":"string"}}}""", "nested", true)]
    [InlineData("""{"type":"object","properties":{"list":{"type":"array","items":{"$ref":"#/components/schemas/Holder"}},"name":{"type":"string"}}}""", "list", true)]
    [InlineData("""{"type":"object","properties":{"name":{"type":"string","writeOnly":false}}}""", null, false)]
    public void Parse_WriteOnlySchema_TreatsPropertyAsSecret(string bodySchema, string? expectedSecret, bool expectedWriteOnly)
    {
        var operation = Assert.Single(OpenApiCliParser.Parse(CreateDocument(bodySchema)));

        Assert.Equal(expectedSecret is null ? [] : [expectedSecret], operation.CliMetadata.SecretProperties);
        Assert.Equal(expectedWriteOnly, operation.RequestBodyHasWriteOnlyValue);
        Assert.Equal(expectedWriteOnly, operation.WriteOnlyProperties.Contains("apiKey"));
    }

    [Fact]
    public void Parse_WriteOnlyInsideAnArrayBody_FlagsTheBodyWithoutProperties()
    {
        var operation = Assert.Single(OpenApiCliParser.Parse(CreateDocument("""{"type":"array","items":{"$ref":"#/components/schemas/Holder"}}""")));

        Assert.Empty(operation.CliMetadata.SecretProperties);
        Assert.True(operation.RequestBodyHasWriteOnlyValue);
    }

    [Fact]
    public void Parse_TwoPropertiesReferenceOneSecretSchema_MarksBoth()
    {
        var operation = Assert.Single(OpenApiCliParser.Parse(CreateDocument(
            """{"type":"object","properties":{"nested":{"type":"object","properties":{"first":{"$ref":"#/components/schemas/Secret"},"second":{"$ref":"#/components/schemas/Secret"}}}}}""")));

        Assert.Contains("first", operation.WriteOnlyProperties);
        Assert.Contains("second", operation.WriteOnlyProperties);
    }

    [Fact]
    public void Parse_ExamplesOnBodiesAndResponses_DoNotChangeTheCommand()
    {
        // Examples hold arbitrary JSON, including what looks like a reference, a schema or a secret.
        const string Example = """{"$ref":"#/components/schemas/Missing","type":"object","properties":{"apiKey":{"type":"string","writeOnly":true}},"password":"Replace-Me-1","list":[1,null,true]}""";
        const string Schema = """{"type":"object","required":["name"],"properties":{"name":{"type":"string"}}}""";
        var withExamples = """
            {"paths":{"/api/widgets":{"post":{"x-oc-cli":{"commandGroup":["widgets"],"verb":"create"},
              "requestBody":{"content":{"application/json":{"schema":SCHEMA,"example":EXAMPLE}}},
              "responses":{"201":{"content":{"application/json":{"schema":{"type":"object"},"example":EXAMPLE}}}}}}}}
            """.Replace("SCHEMA", Schema, StringComparison.Ordinal).Replace("EXAMPLE", Example, StringComparison.Ordinal);

        var operation = Assert.Single(OpenApiCliParser.Parse(withExamples));
        var expected = Assert.Single(OpenApiCliParser.Parse(CreateDocument(Schema)));

        Assert.Equal(expected.RequestBodySchema!.ToJsonString(), operation.RequestBodySchema!.ToJsonString());
        Assert.Equal(expected.RequestBodyProperties.Select(property => property.Name), operation.RequestBodyProperties.Select(property => property.Name));
        Assert.Empty(operation.CliMetadata.SecretProperties);
        Assert.Empty(operation.WriteOnlyProperties);
        Assert.False(operation.RequestBodyHasWriteOnlyValue);
    }

    [Fact]
    public void Parse_RecursiveSchema_Terminates()
    {
        var operation = Assert.Single(OpenApiCliParser.Parse(CreateDocument("""{"$ref":"#/components/schemas/Tree"}""")));

        Assert.False(operation.RequestBodyHasWriteOnlyValue);
    }

    [Fact]
    public void Parse_AlreadyDeclaredSecret_IsNotDuplicated()
    {
        var operation = Assert.Single(OpenApiCliParser.Parse("""
            {"paths":{"/api/widgets":{"post":{"x-oc-cli":{"commandGroup":["widgets"],"verb":"create","secretProperties":["APIKEY"]},
              "requestBody":{"content":{"application/json":{"schema":{"type":"object","properties":{"apiKey":{"type":"string","writeOnly":true}}}}}}}}}}
            """));

        Assert.Equal("APIKEY", Assert.Single(operation.CliMetadata.SecretProperties));
    }

    private static string CreateDocument(string bodySchema) => """
        {
          "paths":{"/api/widgets":{"post":{"x-oc-cli":{"commandGroup":["widgets"],"verb":"create"},
            "requestBody":{"content":{"application/json":{"schema":BODY_SCHEMA}}}}}},
          "components":{"schemas":{
            "Secret":{"type":"string","writeOnly":true},
            "Plain":{"type":"string"},
            "Holder":{"type":"object","properties":{"apiKey":{"type":"string","writeOnly":true}}},
            "Tree":{"type":"object","properties":{"children":{"type":"array","items":{"$ref":"#/components/schemas/Tree"}}}}
          }}
        }
        """.Replace("BODY_SCHEMA", bodySchema, StringComparison.Ordinal);

    [Fact]
    public void Parse_PresentationMetadata_ReadsIt()
    {
        var operation = Assert.Single(OpenApiCliParser.Parse("""
            {"paths":{"/api/sites":{"post":{"x-oc-cli":{
              "commandGroup":["sites"],"verb":"create","resourceName":"Site","summaryProperties":["name","state"],
              "itemsProperty":"results","setupPasswordProperty":"administratorPassword","completedAction":"launched","longRunning":true,"readOnly":true,"requestSchemaPath":"api/sites/{name}/schema",
              "nextSteps":[{"description":"check it.","whenProperty":"state","whenValue":"Running","arguments":["sites","show","{name}"],
                "optionalArguments":[{"whenMissing":"owner","arguments":["--owner","<owner>"]}],"note":"Read this."},{"savesContext":true}]}}}}}
            """));

        Assert.Equal("Site", operation.CliMetadata.ResourceName);
        Assert.Equal(["name", "state"], operation.CliMetadata.SummaryProperties);
        Assert.Equal("results", operation.CliMetadata.ItemsProperty);
        Assert.Equal("administratorPassword", operation.CliMetadata.SetupPasswordProperty);
        Assert.Equal("launched", operation.CliMetadata.CompletedAction);
        Assert.True(operation.CliMetadata.LongRunning);
        Assert.True(operation.CliMetadata.ReadOnly);
        Assert.Equal("api/sites/{name}/schema", operation.CliMetadata.RequestSchemaPath);
        Assert.Equal(2, operation.CliMetadata.NextSteps.Count);
        var step = operation.CliMetadata.NextSteps[0];
        Assert.Equal("check it.", step.Description);
        Assert.Equal("state", step.WhenProperty);
        Assert.Equal("Running", step.WhenValue);
        Assert.Equal(["sites", "show", "{name}"], step.Arguments);
        Assert.Equal("owner", Assert.Single(step.OptionalArguments).WhenMissing);
        Assert.Equal(["--owner", "<owner>"], step.OptionalArguments[0].Arguments);
        Assert.Equal("Read this.", step.Note);
        Assert.False(step.SavesContext);
        Assert.True(operation.CliMetadata.NextSteps[1].SavesContext);
    }

    [Theory]
    [InlineData("#/components/parameters/Loop", "circular")]
    [InlineData("#/components/parameters/Missing", "broken")]
    public void Parse_InvalidParameterReference_ReportsTheDocumentIsBroken(string reference, string expected)
    {
        var document = """
            {
              "paths": {"/api/items": {"get": {
                "operationId": "ApiListItems",
                "x-oc-cli": {"commandGroup": ["items"], "verb": "list"},
                "parameters": [{"$ref": "REFERENCE"}]
              }}},
              "components": {"parameters": {"Loop": {"$ref": "#/components/parameters/Loop"}}}
            }
            """.Replace("REFERENCE", reference, StringComparison.Ordinal);

        Assert.Contains(expected, Assert.Throws<CliException>(() => OpenApiCliParser.Parse(document)).Message);
    }

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
