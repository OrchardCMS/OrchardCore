using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using OrchardCore.BackgroundJobs;
using OrchardCore.BackgroundJobs.Models;
using OrchardCore.BackgroundJobs.Services;
using OrchardCore.Environment.Shell;
using OrchardCore.Locking;
using OrchardCore.Locking.Distributed;
using OrchardCore.Modules;
using Xunit;
using YesSql;

namespace OrchardCore.Tests.Modules.OrchardCore.BackgroundJobs
{
    public class ScheduleJobsBackgroundTaskTests
    {
        [Fact]
        public async Task ShouldPrioritizeJobs()
        {
            var docs = new List<BackgroundJobExecution>
            {
                new BackgroundJobExecution
                {
                    BackgroundJob = new TestBackgroundJob{ BackgroundJobId = "a" },
                    Schedule = new DateTimeSchedule { ExecutionUtc = DateTime.MinValue },
                    State = new BackgroundJobState()
                },
                new BackgroundJobExecution
                {
                    BackgroundJob = new TestBackgroundJob{ BackgroundJobId = "b" },
                    Schedule = new DateTimeSchedule { ExecutionUtc = DateTime.MinValue.AddMinutes(1) },
                    State = new BackgroundJobState()
                }
            };

            var backgroundJobEntriesMock = new Mock<IBackgroundJobEntries>();
            backgroundJobEntriesMock.Setup(x => x.GetEntriesAsync())
                .Returns(() => new ValueTask<IEnumerable<BackgroundJobEntry>>(docs.Select(x => new BackgroundJobEntry(x.BackgroundJob.BackgroundJobId, x.BackgroundJob.Name, x.State.CurrentStatus, x.Schedule.ExecutionUtc))));

            var services = new ServiceCollection()
                .AddSingleton(backgroundJobEntriesMock.Object)
                .AddSingleton<IDistributedLock, LocalLock>()
                .AddScoped<IBackgroundJobScheduleHandler, BackgroundJobScheduleHandler>()
                .AddScoped<ShellSettings>(sp => new ShellSettings { Name = "Test" })
                .AddSingleton<IBackgroundJobQueue, BackgroundJobQueue>()
                .AddScoped<IBackgroundJobSession, BackgroundJobSession>()
                .AddScoped<IBackgroundJobStore, BackgroundJobStore>()
                .AddScoped<ISession>(sp => Mock.Of<ISession>())
                .AddSingleton(Mock.Of<IClock>(s => s.UtcNow == DateTime.MinValue.AddMinutes(2)))
                .AddSingleton(typeof(ILogger<>), typeof(NullLogger<>))
                .BuildServiceProvider();

            var session = services.GetRequiredService<IBackgroundJobSession>();
            foreach (var doc in docs)
            {
                session.Store(doc);
            }

            var backgroundTask = new ScheduleJobsBackgroundTask();
            await backgroundTask.DoWorkAsync(services, default);

            var queue = services.GetRequiredService<IBackgroundJobQueue>();

            var first = await queue.DequeueAsync(default);
            Assert.Equal("a", first.BackgroundJob.BackgroundJobId);
        }
    }

    public class TestBackgroundJob : IBackgroundJob
    {
        public string Name => nameof(TestBackgroundJob);

        public string BackgroundJobId { get; set; }
        public string CorrelationId { get; set; }
        public string RepeatCorrelationId { get; set; }
    }
}
