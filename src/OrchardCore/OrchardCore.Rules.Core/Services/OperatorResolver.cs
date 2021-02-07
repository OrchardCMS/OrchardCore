using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.Rules.Models;

namespace OrchardCore.Rules.Services
{
    public interface IOperatorComparer
    {
        bool Compare(object a, object b);
    }

    public interface IOperatorResolver
    {
        IOperatorComparer GetOperatorComparer(Operator op);
    }

    public class OperatorResolver : IOperatorResolver
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly RuleOptions _ruleOptions;

        public OperatorResolver(IServiceProvider serviceProvider, IOptions<RuleOptions> ruleOptions)
        {
            _serviceProvider = serviceProvider;
            _ruleOptions = ruleOptions.Value;
        }

        public IOperatorComparer GetOperatorComparer(Operator op)
        {
            if (_ruleOptions.Comparers.TryGetValue(op.GetType(), out var methodEvaluatorType))
            {
                return _serviceProvider.GetRequiredService(methodEvaluatorType) as IOperatorComparer;
            }

            throw new InvalidOperationException($"Operator comparer for '{op.GetType().Name}; not registered");
        }
    }
}
