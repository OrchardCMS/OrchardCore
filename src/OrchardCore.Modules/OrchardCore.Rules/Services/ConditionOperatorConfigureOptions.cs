using System.Collections.Generic;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Localization;
using OrchardCore.Rules.Models;

namespace OrchardCore.Rules.Services
{
    public class ConditionOperatorConfigureOptions : IConfigureOptions<ConditionOperatorOptions>
    {
        private readonly IStringLocalizer S;

        public ConditionOperatorConfigureOptions(IStringLocalizer<ConditionOperatorOptions> stringLocalizer)
        {
            S = stringLocalizer;
        }

        public void Configure(ConditionOperatorOptions options)
        {
            options.Operators.AddRange(new List<ConditionOperatorOption>
            {
                new ConditionOperatorOption
                {
                    DisplayText = S["Equals"],
                    Operator = typeof(StringEqualsOperator),
                    Comparer = new StringEqualsOperatorComparer(),
                    Factory = new ConditionOperatorFactory<StringEqualsOperator>()
                },
                new ConditionOperatorOption
                {
                    DisplayText = S["Does Not Equal"],
                    Operator = typeof(StringNotEqualsOperator),
                    Comparer = new StringNotEqualsOperatorComparer(),
                    Factory = new ConditionOperatorFactory<StringNotEqualsOperator>()
                },                
                new ConditionOperatorOption
                {
                    DisplayText = S["Starts with"],
                    Operator = typeof(StringStartsWithOperator),
                    Comparer = new StringStartsWithOperatorComparer(),
                    Factory = new ConditionOperatorFactory<StringStartsWithOperator>()
                },           
                new ConditionOperatorOption
                {
                    DisplayText = S["Does Not Start with"],
                    Operator = typeof(StringNotStartsWithOperator),
                    Comparer = new StringNotStartsWithOperatorComparer(),
                    Factory = new ConditionOperatorFactory<StringNotStartsWithOperator>()
                },
                new ConditionOperatorOption
                {
                    DisplayText = S["Ends with"],
                    Operator = typeof(StringEndsWithOperator),
                    Comparer = new StringEndsWithOperatorComparer(),
                    Factory = new ConditionOperatorFactory<StringEndsWithOperator>()
                },
                new ConditionOperatorOption
                {
                    DisplayText = S["Does Not End with"],
                    Operator = typeof(StringNotEndsWithOperator),
                    Comparer = new StringNotEndsWithOperatorComparer(),
                    Factory = new ConditionOperatorFactory<StringNotEndsWithOperator>()
                },                
                new ConditionOperatorOption
                {
                    DisplayText = S["Contains"],
                    Operator = typeof(StringContainsOperator),
                    Comparer = new StringContainsOperatorComparer(),
                    Factory = new ConditionOperatorFactory<StringContainsOperator>()
                },
                new ConditionOperatorOption
                {
                    DisplayText = S["Does Not Contain"],
                    Operator = typeof(StringNotContainsOperator),
                    Comparer = new StringNotContainsOperatorComparer(),
                    Factory = new ConditionOperatorFactory<StringNotContainsOperator>()
                }                
            });
        }
    }
}
