using GraphQL.Language.AST;
using GraphQL.Validation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace OrchardCore.Apis.GraphQL.ValidationRules
{
    public class MaxNumberOfResultsValidationRule : IValidationRule
    {
        private readonly int _maxResults;
        private readonly MaxNumberOfResultsValidationMode _maxNumberOfResultsValidationMode;

        public MaxNumberOfResultsValidationRule(int maxNumberOfResults, MaxNumberOfResultsValidationMode maxNumberOfResultsValidationMode)
        {
            _maxResults = maxNumberOfResults;
            _maxNumberOfResultsValidationMode = maxNumberOfResultsValidationMode;
        }

        public INodeVisitor Validate(ValidationContext validationContext)
        {
            return new EnterLeaveListener(_ =>
            {
                _.Match<Argument>(arg =>
                {
                    if ((arg.Name == "first" || arg.Name == "last") && arg.Value != null)
                    {
                        var context = (GraphQLContext)validationContext.UserContext;

                        var value = (IntValue)arg.Value;

                        if (value?.Value > _maxResults)
                        {
                            var localizer = context.ServiceProvider.GetService<IStringLocalizer<MaxNumberOfResultsValidationRule>>();
                            var errorMessage = localizer[$"'{value}' exceeds the maximum number of results for '{arg.Name}' ({_maxResults})"];

                            if (_maxNumberOfResultsValidationMode == MaxNumberOfResultsValidationMode.Debug)
                            {
                                validationContext.ReportError(new ValidationError(
                                    validationContext.OriginalQuery,
                                    "UserInputError",
                                    errorMessage,
                                    arg));
                            }
                            else
                            {
                                var logger = context.ServiceProvider.GetService<ILogger<MaxNumberOfResultsValidationMode>>();
                                logger.LogInformation(errorMessage);
                                arg.Value = new IntValue(_maxResults); // in release mode we just log info andf override the arg to be maxvalue
                            }
                        }
                    }
                });
            });
        }
    }
}
