using System;
using OrchardCore.BackgroundJobs.Models;
using YesSql.Indexes;

namespace OrchardCore.BackgroundJobs.Indexes
{
    public class BackgroundJobIndex : MapIndex
    {
        public string BackgroundJobId { get; set; }
        public string Name { get; set; }
        public string RepeatCorrelationId { get; set; }
        public string CorrelationId { get; set; }
        public DateTime? CreatedUtc { get; set; }
        public BackgroundJobStatus Status { get; set; }
        public DateTime? ExecutionUtc { get; set; }
    }

    public class BackgroundJobIndexProvider : IndexProvider<BackgroundJobExecution>
    {
        public override void Describe(DescribeContext<BackgroundJobExecution> context)
        {
            context.For<BackgroundJobIndex>()
                .Map(backgroundJob =>
                    new BackgroundJobIndex
                    {
                        BackgroundJobId = backgroundJob.BackgroundJob.BackgroundJobId,
                        Name = backgroundJob.BackgroundJob.Name,
                        RepeatCorrelationId = backgroundJob.BackgroundJob.RepeatCorrelationId,
                        CorrelationId = backgroundJob.BackgroundJob.CorrelationId,
                        CreatedUtc = backgroundJob.State.CreatedUtc,
                        Status = backgroundJob.State.CurrentStatus,
                        ExecutionUtc = backgroundJob.Schedule.ExecutionUtc
                    });
        }
    }
}
