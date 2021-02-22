using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.Rules;

namespace OrchardCore.Rules.Services
{
    public interface IConditionResolver
    {
        IConditionEvaluator GetConditionEvaluator(Condition condition);
    }
}
