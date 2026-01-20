using OrchardCore.Environment.Shell.Models;
using OrchardCore.Tenants.Services;

namespace OrchardCore.Modules.OrchardCore.Tenants.Tests;

public class ProfilesTests
{
    [Fact]
    public void FeatureProfilesSchemaService_ShouldCreateValidSchema()
    {
        var featureProfilesRuleOptions = new FeatureProfilesRuleOptions();
        featureProfilesRuleOptions.Rules["Exclude"] = static (expression, name) => (true, true);
        var options = Options.Create(featureProfilesRuleOptions);

        var service = new FeatureProfilesSchemaService(options);

        var expectedSchema = """
            {
              "$schema": "http://json-schema.org/draft-04/schema#",
              "title": "Feature rules",
              "type": "array",
              "description": "An array of feature rules",
              "items": {
                "$ref": "#/definitions/FeatureRule"
              },
              "definitions": {
                "FeatureRule": {
                  "type": "object",
                  "additionalProperties": false,
                  "required": [
                    "Rule",
                    "Expression"
                  ],
                  "properties": {
                    "Rule": {
                      "minLength": 1,
                      "$ref": "#/definitions/Rule"
                    },
                    "Expression": {
                      "type": "string",
                      "minLength": 1
                    }
                  }
                },
                "Rule": {
                  "type": "string",
                  "description": "The rule to apply to this expression",
                  "enum": [
                    "Exclude"
                  ]
                }
              }
            }
            """;

        Assert.Equal(expectedSchema, service.GetJsonSchema());
    }
}
