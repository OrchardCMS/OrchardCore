using System.Collections.Generic;
using System.Linq;

namespace OrchardCore.Workflows.Activities
{
    public class ActivityExecutionResult
    {
        public static ActivityExecutionResult Noop()
        {
            return new ActivityExecutionResult();
        }

        public static ActivityExecutionResult FromOutcomes(IEnumerable<string> outcomes)
        {
            return new ActivityExecutionResult { Outcomes = outcomes.ToList() };
        }

        public static ActivityExecutionResult Halt()
        {
            return new ActivityExecutionResult { IsHalted = true };
        }

        private ActivityExecutionResult()
        {
        }

        public IList<string> Outcomes { get; private set; } = new List<string>(0);
        public bool IsHalted { get; set; }
    }
}
