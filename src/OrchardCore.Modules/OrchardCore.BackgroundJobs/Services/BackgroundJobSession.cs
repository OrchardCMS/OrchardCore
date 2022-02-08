using System;
using System.Collections.Generic;
using OrchardCore.BackgroundJobs.Models;

namespace OrchardCore.BackgroundJobs.Services
{
    public class BackgroundJobSession : IBackgroundJobSession
    {
        private Dictionary<string, BackgroundJobExecution> _backgroundJobExecutionsById;
        private Dictionary<string, BackgroundJobExecution> _backgroundJobExecutionsByCorrelationId;

        public void Store(BackgroundJobExecution backgroundJobExecution)
        {
            _backgroundJobExecutionsById ??= new Dictionary<string, BackgroundJobExecution>();

            _backgroundJobExecutionsById[backgroundJobExecution.BackgroundJob.BackgroundJobId] = backgroundJobExecution;
            if (!String.IsNullOrEmpty(backgroundJobExecution.BackgroundJob.CorrelationId))
            {
                _backgroundJobExecutionsByCorrelationId ??= new Dictionary<string, BackgroundJobExecution>();
                _backgroundJobExecutionsByCorrelationId[backgroundJobExecution.BackgroundJob.BackgroundJobId] = backgroundJobExecution;
            }
        }

        public bool TryRecallBackgroundJobById(string backgroundJobId, out BackgroundJobExecution backgroundJobExecution)
        {
            if (_backgroundJobExecutionsById is null)
            {
                backgroundJobExecution = null;
                return false;
            }

            return _backgroundJobExecutionsById.TryGetValue(backgroundJobId, out backgroundJobExecution);
        }

        public bool TryRecallBackgroundJobByCorrelationId(string correlationId, out BackgroundJobExecution backgroundJobExecution)
        {
            if (String.IsNullOrEmpty(correlationId) || _backgroundJobExecutionsByCorrelationId is null)
            {
                backgroundJobExecution = null;
                return false;
            }

            return _backgroundJobExecutionsByCorrelationId.TryGetValue(correlationId, out backgroundJobExecution);
        }
    }
}
