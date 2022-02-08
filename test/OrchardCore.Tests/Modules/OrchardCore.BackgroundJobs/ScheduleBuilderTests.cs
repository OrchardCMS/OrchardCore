using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using OrchardCore.BackgroundJobs;
using OrchardCore.BackgroundJobs.Services;
using OrchardCore.Modules;
using Xunit;

namespace OrchardCore.Tests.Modules.OrchardCore.BackgroundJobs
{
    public class ScheduleBuilderTests
    {
        [Fact]
        public async Task ShouldCreateNowSchedule()
        {
            var clock = Mock.Of<IClock>(s => s.UtcNow == DateTime.MaxValue);

            var services = CreateServices()
                .AddSingleton(clock)
                .BuildServiceProvider();

            var schedule = await services.GetRequiredService<IScheduleBuilderFactory>().Now().BuildAsync();

            Assert.Equal(DateTime.MaxValue, schedule.ExecutionUtc);
        }

        [Fact]
        public async Task ShouldCreateDelaySchedule()
        {
            var clock = Mock.Of<IClock>(s => s.UtcNow == DateTime.MinValue);

            var services = CreateServices()
                .AddSingleton(clock)
                .BuildServiceProvider();

            var schedule = await services.GetRequiredService<IScheduleBuilderFactory>().Delay(TimeSpan.FromSeconds(30)).BuildAsync();

            Assert.Equal(DateTime.MinValue.AddSeconds(30), schedule.ExecutionUtc);
        }

        [Fact]
        public async Task ShouldCreateUtcSchedule()
        {
            var clock = Mock.Of<IClock>(s => s.UtcNow == DateTime.MinValue);

            var services = CreateServices()
                .AddSingleton(clock)
                .BuildServiceProvider();

            var schedule = await services.GetRequiredService<IScheduleBuilderFactory>().Utc(DateTime.MinValue.AddSeconds(30)).BuildAsync();

            Assert.Equal(DateTime.MinValue.AddSeconds(30), schedule.ExecutionUtc);
        }

        [Fact]
        public async Task ShouldCreateCrontabSchedule()
        {
            var clock = Mock.Of<IClock>(s => s.UtcNow == DateTime.MinValue);

            var services = CreateServices()
                .AddSingleton(clock)
                .BuildServiceProvider();

            var schedule = await services.GetRequiredService<IScheduleBuilderFactory>().Crontab("* * * * *").BuildAsync();

            Assert.Equal(DateTime.MinValue.AddMinutes(1), schedule.ExecutionUtc);
        }

        [Fact]
        public async Task ShouldCreateRepeatCrontabSchedule()
        {
            var clock = Mock.Of<IClock>(s => s.UtcNow == DateTime.MinValue);

            var services = CreateServices()
                .AddSingleton(clock)
                .BuildServiceProvider();

            var schedule = await services.GetRequiredService<IScheduleBuilderFactory>().Crontab("* * * * *").Repeat().BuildAsync();

            Assert.Equal(DateTime.MinValue.AddMinutes(1), schedule.ExecutionUtc);
        }

        [Fact]
        public async Task ShouldCreateScheduleStartsNowRepeatsWithCrontab()
        {
            var clock = Mock.Of<IClock>(s => s.UtcNow == DateTime.MaxValue);

            var services = CreateServices()
                .AddSingleton(clock)
                .BuildServiceProvider();

            var schedule = await services.GetRequiredService<IScheduleBuilderFactory>()
                    .Now()
                    .Repeat("* * * * *")
                    .BuildAsync();

            Assert.Equal(DateTime.MaxValue, schedule.ExecutionUtc);
        }

        private static IServiceCollection CreateServices()
            => new ServiceCollection()
                .AddSingleton<IScheduleBuilderFactory, ScheduleBuilderFactory>();
    }
}
