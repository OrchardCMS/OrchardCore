using System.Threading.Tasks;

namespace OrchardCore.BackgroundJobs.Events
{
    public interface IBackgroundJobEvent
    {
        ValueTask CreatedAsync(BackgroundJobCreateContext context);
        ValueTask CreatingAsync(BackgroundJobCreateContext context);
        ValueTask DeletedAsync(BackgroundJobDeleteContext context);
        ValueTask DeletingAsync(BackgroundJobDeleteContext context);
        ValueTask UpdatedAsync(BackgroundJobUpdateContext context);
        ValueTask UpdatingAsync(BackgroundJobUpdateContext context);
        ValueTask ExecutingAsync(BackgroundJobExecutionContext context);
        ValueTask ExecutedAsync(BackgroundJobExecutionContext context);
        ValueTask FailingAsync(BackgroundJobFailureContext context);
        ValueTask FailedAsync(BackgroundJobFailureContext context);
        ValueTask SchedulingAsync(BackgroundJobScheduleContext context);
        ValueTask ScheduledAsync(BackgroundJobScheduleContext context);
    }
}
