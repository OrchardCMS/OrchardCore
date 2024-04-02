using System;
using System.Collections.Generic;
using OrchardCore.Scripting;

namespace OrchardCore.Workflows.Scripting
{
    public class OutcomeMethodProvider : IGlobalMethodProvider
    {
        private readonly GlobalMethod _setOutcomeMethod;

        public OutcomeMethodProvider(IList<string> outcomes)
        {
            _setOutcomeMethod = new GlobalMethod
            {
                Name = "setOutcome",
                Method = serviceProvider => (Action<string>)(name => outcomes.Add(name)),
            };
        }

        public IEnumerable<GlobalMethod> GetMethods()
        {
            return new[] { _setOutcomeMethod };
        }
    }
}
