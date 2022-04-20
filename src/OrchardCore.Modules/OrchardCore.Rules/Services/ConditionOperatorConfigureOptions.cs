using System.Collections.Generic;
using Microsoft.Extensions.Options;
using OrchardCore.Rules.Models;

namespace OrchardCore.Rules.Services
{
    public class ConditionOperatorConfigureOptions : IConfigureOptions<ConditionOperatorOptions>
    {
        public void Configure(ConditionOperatorOptions options)
        {
            options.Operators.AddRange(new List<ConditionOperatorOption>
            {
                new ConditionOperatorOption<ConditionOperatorConfigureOptions>(
                    (S) => S["Equals"],
                    new StringEqualsOperatorComparer(),
                    typeof(StringEqualsOperator),
                    new ConditionOperatorFactory<StringEqualsOperator>()
                ),
                new ConditionOperatorOption<ConditionOperatorConfigureOptions>(
                    (S) => S["Does not equal"],
                    new StringNotEqualsOperatorComparer(),
                    typeof(StringNotEqualsOperator),
                    new ConditionOperatorFactory<StringNotEqualsOperator>()
                ),
                new ConditionOperatorOption<ConditionOperatorConfigureOptions>(
                    (S) => S["Starts with"],
                    new StringStartsWithOperatorComparer(),
                    typeof(StringStartsWithOperator),
                    new ConditionOperatorFactory<StringStartsWithOperator>()
                ),
                new ConditionOperatorOption<ConditionOperatorConfigureOptions>(
                    (S) => S["Does not start with"],
                    new StringNotStartsWithOperatorComparer(),
                    typeof(StringNotStartsWithOperator),
                    new ConditionOperatorFactory<StringNotStartsWithOperator>()
                ),
                new ConditionOperatorOption<ConditionOperatorConfigureOptions>(
                    (S) => S["Ends with"],
                    new StringEndsWithOperatorComparer(),
                    typeof(StringEndsWithOperator),
                    new ConditionOperatorFactory<StringEndsWithOperator>()
                ),
                new ConditionOperatorOption<ConditionOperatorConfigureOptions>(
                    (S) => S["Does not end with"],
                    new StringNotEndsWithOperatorComparer(),
                    typeof(StringNotEndsWithOperator),
                    new ConditionOperatorFactory<StringNotEndsWithOperator>()
                ),
                new ConditionOperatorOption<ConditionOperatorConfigureOptions>(
                    (S) => S["Contains"],
                    new StringContainsOperatorComparer(),
                    typeof(StringContainsOperator),
                    new ConditionOperatorFactory<StringContainsOperator>()
                ),
                new ConditionOperatorOption<ConditionOperatorConfigureOptions>(
                    (S) => S["Does not contain"],
                    new StringNotContainsOperatorComparer(),
                    typeof(StringNotContainsOperator),
                    new ConditionOperatorFactory<StringNotContainsOperator>()
                )
            });
        }
    }
}
