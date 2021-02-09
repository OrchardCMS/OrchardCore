using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Orchard.Rules.Drivers;
using OrchardCore.Rules.Models;

namespace OrchardCore.Rules.Services
{
    public class ContentTypeConditionEvaluator : ConditionEvaluator<ContentTypeCondition>
    {
        private readonly IDisplayedContentItemDriver _displayContentItemDriver;
        private readonly IConditionOperatorResolver _operatorResolver;

        public ContentTypeConditionEvaluator(IDisplayedContentItemDriver displayContentItemDriver, IConditionOperatorResolver operatorResolver)
        {
            _displayContentItemDriver = displayContentItemDriver;
            _operatorResolver = operatorResolver;
        }

        public override ValueTask<bool> EvaluateAsync(ContentTypeCondition condition)
        {
            if (_displayContentItemDriver.IsDisplayed(condition.Value))
            {
                return new ValueTask<bool>(true);
            }

            return new ValueTask<bool>(false);
            // var operatorResolver = _operatorResolver.GetOperatorComparer(condition.Operation);
            // return new ValueTask<bool>(operatorResolver.Compare(condition.Operation, requestPath, condition.Value));
        }
    }
}