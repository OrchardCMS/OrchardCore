using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using NCrontab;
using OrchardCore.Modules;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Workflows.Timers
{
    public class TimerEvent : EventActivity
    {
        public static string EventName => nameof(TimerEvent);
        private readonly IClock _clock;
        private readonly IStringLocalizer S;

        public TimerEvent(IClock clock, IStringLocalizer<TimerEvent> localizer)
        {
            _clock = clock;
            S = localizer;
        }

        public override string Name => EventName;

        public override LocalizedString DisplayText => S["Timer Event"];

        public override LocalizedString Category => S["Background"];

        public string CronExpression
        {
            get => GetProperty(() => "*/5 * * * *");
            set => SetProperty(value);
        }

        public DateTime? StartAtUtc
        {
            get => GetProperty<DateTime?>();
            set => SetProperty(value);
        }

        private DateTime? StartedUtc
        {
            get => GetProperty<DateTime?>();
            set => SetProperty(value);
        }

        private string OriginalCorrelationId
        {
            get => GetProperty<string>();
            set => SetProperty(value);
        }

        public override bool CanExecute(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            return StartedUtc == null || IsExpired();
        }

        public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            return Outcomes(S["Done"]);
        }

        public override ActivityExecutionResult Execute(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            if (OriginalCorrelationId == null)
            {
                OriginalCorrelationId = workflowContext.CorrelationId;
                workflowContext.CorrelationId = null;
            }

            return Halt();
        }

        public override ActivityExecutionResult Resume(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            if (IsExpired())
            {
                if (OriginalCorrelationId != null)
                {
                    workflowContext.CorrelationId = OriginalCorrelationId;
                }

                workflowContext.LastResult = "TimerEvent";
                return Outcomes("Done");
            }

            return Halt();
        }

        private bool IsExpired()
        {
            if (StartedUtc == null)
            {
                StartedUtc = StartAtUtc ?? _clock.UtcNow;
            }

            var schedule = CrontabSchedule.Parse(CronExpression);
            var whenUtc = schedule.GetNextOccurrence(StartedUtc.Value);

            return _clock.UtcNow >= whenUtc;
        }
    }
}
