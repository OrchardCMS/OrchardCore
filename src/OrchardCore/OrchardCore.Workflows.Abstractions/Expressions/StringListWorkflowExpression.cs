using System;
using System.Collections.Generic;
using System.Linq;

namespace OrchardCore.Workflows.Models
{
    public class StringListWorkflowExpression : WorkflowExpression<IList<string>>
    {
        public StringListWorkflowExpression()
        {
        }

        public StringListWorkflowExpression(string expression) : base(expression)
        {
        }

        public override IList<string> Parse()
        {
            return string.IsNullOrWhiteSpace(Expression) ? Expression.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToList() : new List<string>(0);
        }

        public override IList<string> ProcessScriptResult(object result)
        {
            var items = (IEnumerable<object>)result;
            return items?.Select(x => x.ToString()).ToList();
        }
    }
}
