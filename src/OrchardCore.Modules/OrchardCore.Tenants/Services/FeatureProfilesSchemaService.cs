using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using NJsonSchema;
using OrchardCore.Environment.Shell.Models;

namespace OrchardCore.Tenants.Services
{
    /// <summary>
    /// Generates a json schema for a <see cref="FeatureRule"/>
    /// </summary>
    public class FeatureProfilesSchemaService : IFeatureProfilesSchemaService
    {
        private readonly FeatureProfilesRuleOptions _featureProfilesRuleOptions;
        private IHostEnvironment _hostEnvironment;
        private readonly IStringLocalizer S;

        public FeatureProfilesSchemaService(
            IOptions<FeatureProfilesRuleOptions> options,
            IHostEnvironment hostEnvironment,
            IStringLocalizer<FeatureProfilesSchemaService> stringLocalizer)
        {
            _hostEnvironment = hostEnvironment;
            _featureProfilesRuleOptions = options.Value;
            S = stringLocalizer;
        }

        public string GetJsonSchema()
        {
            // Generate a schema, then localize and mutate the schema to add dynamic properties.
            var schema = JsonSchema.FromType<FeatureRule[]>();

            schema.Title = S["Feature rules"];
            schema.Description = S["An array of feature rules"];

            if (schema.Definitions.TryGetValue(nameof(FeatureRule), out var featureRule) && featureRule.ActualProperties.TryGetValue(nameof(FeatureRule.Rule), out var rule))
            {
                var ruleProperty = new JsonSchema()
                {
                    Type = JsonObjectType.String,
                    Description = S["The rule to apply to this expression"]
                };

                foreach(var ruleOption in _featureProfilesRuleOptions.Rules.Keys)
                {
                    ruleProperty.Enumeration.Add(ruleOption);
                }

                schema.Definitions.Add(nameof(FeatureRule.Rule), ruleProperty);
                rule.Reference = ruleProperty;
            }

            if (_hostEnvironment.IsDevelopment())
            {
                return schema.ToJson(Formatting.Indented);
            }

            return schema.ToJson();
        }
    }
}
