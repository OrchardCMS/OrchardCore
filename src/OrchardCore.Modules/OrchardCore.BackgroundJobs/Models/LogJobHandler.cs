using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OrchardCore.Modules;

namespace OrchardCore.BackgroundJobs.Models
{

    public class LogJob2 : BackgroundJob<LogJob2>
    {
        public string Message { get; set; }
    }

    public class LogJobHandler : BackgroundJobHandler<LogJob2>
    {
        private readonly ILogger _logger;
        private readonly IBackgroundJobService _backgroundJobService;

        public LogJobHandler(ILogger<LogJobHandler> logger, IBackgroundJobService backgroundJobService)
        {
            _logger = logger;
            _backgroundJobService = backgroundJobService;
         }

        public override async ValueTask ExecuteAsync(LogJob2 backgroundJob, CancellationToken cancellationToken)
        {
            await Task.Delay(5000);

            var job = await _backgroundJobService.GetByIdAsync(backgroundJob.BackgroundJobId);

            _logger.LogError(String.Format(backgroundJob.Message, job.State.CreatedUtc.ToString()));
            //throw new Exception("fubar");

            //return new ValueTask();

        }
    }

}
