using System;
using System.Collections.Generic;
using OrchardCore.BackgroundJobs.Models;

namespace OrchardCore.BackgroundJobs.Events
{
    public abstract class BackgroundJobBaseContext
    {
        protected BackgroundJobBaseContext(BackgroundJobExecution backgroundJobExecution)
        {
            BackgroundJobExecution = backgroundJobExecution;
        }

        public BackgroundJobExecution BackgroundJobExecution { get; set; }

    }
    public class BackgroundJobCreateContext : BackgroundJobBaseContext
    {
        public BackgroundJobCreateContext(BackgroundJobExecution backgroundJobExecution) : base(backgroundJobExecution)
        {
        }
    }

    public class BackgroundJobDeleteContext : BackgroundJobBaseContext
    {
        public BackgroundJobDeleteContext(BackgroundJobExecution backgroundJobExecution) : base(backgroundJobExecution)
        {
        }

        public bool Cancel { get; set; }
    }

    public class BackgroundJobUpdateContext : BackgroundJobBaseContext
    {
        public BackgroundJobUpdateContext(BackgroundJobExecution backgroundJobExecution) : base(backgroundJobExecution)
        {
        }
    }

    public class BackgroundJobExecutionContext : BackgroundJobBaseContext
    {
        public BackgroundJobExecutionContext(BackgroundJobExecution backgroundJobExecution) : base(backgroundJobExecution)
        {
        }
    }

    public class BackgroundJobFailureContext : BackgroundJobBaseContext
    {
        public BackgroundJobFailureContext(Exception exception, BackgroundJobExecution backgroundJobExecution) : base(backgroundJobExecution)
        {
            Exception = exception;
        }

        public Exception Exception { get; set; }
    }

    public class BackgroundJobScheduleContext : BackgroundJobBaseContext
    {
        public BackgroundJobScheduleContext(BackgroundJobExecution backgroundJobExecution, long priority, IList<string> executingNames) : base(backgroundJobExecution)
        {
            Priority = priority;
            ExecutingNames = executingNames;
        }

        public bool CanRun { get; set; } = true;
        public long Priority { get; set; }
        public IList<string> ExecutingNames { get; set; }
    }
}
