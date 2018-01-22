using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Workflows.Activities
{
    public class TryCatchTask : TaskActivity
    {
        private readonly ILogger<TryCatchTask> _logger;

        public TryCatchTask(ILogger<TryCatchTask> logger, IStringLocalizer<TryCatchTask> localizer)
        {
            _logger = logger;
            T = localizer;
        }

        private IStringLocalizer T { get; }
        public override string Name => nameof(TryCatchTask);
        public override LocalizedString Category => T["Primitives"];

        public string ExceptionTypes
        {
            get => GetProperty(() => "System.Exception");
            set => SetProperty(value);
        }

        public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            var supportedExceptions = ParseExceptionTypes(ExceptionTypes);
            var exceptionOutcomes =
                from exceptionType in supportedExceptions
                select new Outcome(T[exceptionType.FullName]);

            return Outcomes(T["Try"]).Concat(exceptionOutcomes);
        }

        public override ActivityExecutionResult Execute(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            var exceptionTypeName = GetProperty<string>(name: "HandledExceptionType");
            if (exceptionTypeName != null)
            {
                var exceptionType = Type.GetType(exceptionTypeName);
                return Outcomes(exceptionType.FullName);
            }
            else
            {
                return Outcomes("Try");
            }
        }

        public override bool HandleException(WorkflowExecutionContext workflowContext, ActivityContext activityContext, Exception exception)
        {
            var supportedExceptionTypes = ParseExceptionTypes(ExceptionTypes).ToList();

            var matchingException =
                supportedExceptionTypes.FirstOrDefault(x => x == exception.GetType())
                ?? supportedExceptionTypes.FirstOrDefault(x => x.IsInstanceOfType(exception));

            if (matchingException != null)
            {
                SetProperty(exception.GetType().AssemblyQualifiedName, "HandledExceptionType");
                SetProperty(exception, "HandledException");
                return true;
            }

            return false;
        }

        private IEnumerable<Type> ParseExceptionTypes(string types)
        {
            if (string.IsNullOrWhiteSpace(types))
                return Enumerable.Empty<Type>();

            return
                from typeName in types.Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim())
                let type = Type.GetType(typeName)
                where type != null
                select type;
        }
    }
}