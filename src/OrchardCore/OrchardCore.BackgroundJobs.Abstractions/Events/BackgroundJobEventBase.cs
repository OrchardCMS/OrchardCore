using System.Threading.Tasks;

namespace OrchardCore.BackgroundJobs.Events
{
    public class BackgroundJobEventBase : IBackgroundJobEvent
    {
        public virtual ValueTask CreatingAsync(BackgroundJobCreateContext context) => new ValueTask();
        public virtual ValueTask CreatedAsync(BackgroundJobCreateContext context) => new ValueTask();

        public virtual ValueTask DeletingAsync(BackgroundJobDeleteContext context) => new ValueTask();
        public virtual ValueTask DeletedAsync(BackgroundJobDeleteContext context) => new ValueTask();

        public virtual ValueTask UpdatingAsync(BackgroundJobUpdateContext context) => new ValueTask();
        public virtual ValueTask UpdatedAsync(BackgroundJobUpdateContext context) => new ValueTask();

        public virtual ValueTask ExecutingAsync(BackgroundJobExecutionContext context) => new ValueTask();
        public virtual ValueTask ExecutedAsync(BackgroundJobExecutionContext context) => new ValueTask();

        public virtual ValueTask SchedulingAsync(BackgroundJobScheduleContext context) => new ValueTask();
        public virtual ValueTask ScheduledAsync(BackgroundJobScheduleContext context) => new ValueTask();

        public virtual ValueTask FailingAsync(BackgroundJobFailureContext context) => new ValueTask();
        public virtual ValueTask FailedAsync(BackgroundJobFailureContext context) => new ValueTask();
    }
}
