using System;
using System.Collections.Generic;

namespace OrchardCore.Workflows.Activities
{
    public class ActivityExecutionResult
    {
        public static readonly ActivityExecutionResult Empty = new(Array.Empty<string>());

        public static readonly ActivityExecutionResult Halted = new(Array.Empty<string>()) { IsHalted = true };

        public ActivityExecutionResult(IEnumerable<string> outcomes)
        {
            Outcomes = outcomes;
        }

        public ActivityExecutionResult(string[] outcomes, bool halted)
        {
            Outcomes = outcomes;
            IsHalted = halted;
        }

        public IEnumerable<string> Outcomes { get; private set; }
        public bool IsHalted { get; private set; }
    }
}
