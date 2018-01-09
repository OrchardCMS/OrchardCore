using System;
using System.Collections.Generic;
using System.Linq;

namespace OrchardCore.Workflows.Models
{
    public class StringWorkflowExpression : WorkflowExpression<string>
    {
        public StringWorkflowExpression()
        {
        }

        public StringWorkflowExpression(string expression) : base(expression)
        {
        }

        public override string Parse()
        {
            return Expression;
        }
    }
}
