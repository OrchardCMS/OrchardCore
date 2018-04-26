using System;
using System.Collections.Generic;
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

        public TimerEvent(IClock clock, IStringLocalizer<TimerEvent> localizer)
        {
            _clock = clock;
            T = localizer;
        }

        private IStringLocalizer T { get; }

        public override string Name => EventName;
        public override LocalizedString Category => T["Background"];

        public string CronExpression
        {
            get => GetProperty(() => "5 * * * *");
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

        public override bool CanExecute(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            return StartedUtc == null || IsExpired();
        }

        public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            return Outcomes(T["Done"]);
        }

        public override ActivityExecutionResult Resume(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            if (IsExpired())
            {
                workflowContext.LastResult = "TimerEvent";
                return Outcomes("Done");
            }

            return Halt();
        }

        private bool IsExpired()
        {
            var startedUtc = StartedUtc;

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