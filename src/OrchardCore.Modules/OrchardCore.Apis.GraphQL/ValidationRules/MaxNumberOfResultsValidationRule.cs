using System.Threading.Tasks;
using GraphQL.Language.AST;
using GraphQL.Validation;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace OrchardCore.Apis.GraphQL.ValidationRules
{
    public class MaxNumberOfResultsValidationRule : IValidationRule
    {
        private readonly int _maxNumberOfResults;
        private readonly MaxNumberOfResultsValidationMode _maxNumberOfResultsValidationMode;
        protected readonly IStringLocalizer S;
        private readonly ILogger _logger;

        public MaxNumberOfResultsValidationRule(
            IOptions<GraphQLSettings> options,
            IStringLocalizer<MaxNumberOfResultsValidationRule> localizer,
            ILogger<MaxNumberOfResultsValidationRule> logger)
        {
            var settings = options.Value;
            _maxNumberOfResults = settings.MaxNumberOfResults;
            _maxNumberOfResultsValidationMode = settings.MaxNumberOfResultsValidationMode;
            S = localizer;
            _logger = logger;
        }

        public Task<INodeVisitor> ValidateAsync(ValidationContext validationContext)
        {
            return Task.FromResult((INodeVisitor)new NodeVisitors(
            new MatchingNodeVisitor<Argument>((arg, visitorContext) =>
            {
                if ((arg.Name == "first" || arg.Name == "last") && arg.Value != null)
                {
                    var context = (GraphQLUserContext)validationContext.UserContext;

                    int? value = null;

                    if (arg.Value is IntValue)
                    {
                        value = ((IntValue)arg.Value)?.Value;
                    }
                    else
                    {
                        if (validationContext.Inputs.TryGetValue(arg.Value.ToString(), out var input))
                        {
                            value = (int?)input;
                        }
                    }

                    if (value.HasValue && value > _maxNumberOfResults)
                    {
                        var errorMessage = S["'{0}' exceeds the maximum number of results for '{1}' ({2})", value.Value, arg.Name, _maxNumberOfResults];

                        if (_maxNumberOfResultsValidationMode == MaxNumberOfResultsValidationMode.Enabled)
                        {
                            validationContext.ReportError(new ValidationError(
                                validationContext.Document.OriginalQuery,
                                "ArgumentInputError",
                                errorMessage,
                                arg));
                        }
                        else
                        {
                            _logger.LogInformation("'{IntValue}' exceeds the maximum number of results for '{ArgumentName}' ({MaxNumber})",
                                value.Value,
                                arg.Name,
                                _maxNumberOfResults);

                            // If disabled mode we just log info and override the arg to be maxvalue.
                            arg = new Argument(arg.NameNode, new IntValue(_maxNumberOfResults));
                        }
                    }
                }
            })));
        }
    }
}
