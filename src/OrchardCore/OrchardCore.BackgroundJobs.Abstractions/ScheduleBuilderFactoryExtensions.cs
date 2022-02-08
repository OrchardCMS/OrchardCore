using System;
using OrchardCore.BackgroundJobs.Schedules;

namespace OrchardCore.BackgroundJobs
{
    public static class ScheduleBuilderFactoryExtensions
    {
        public static IScheduleBuilder Now(this IScheduleBuilderFactory factory)
            => factory.Create<NowScheduleBuilder>();

        public static IScheduleBuilder Delay(this IScheduleBuilderFactory factory, TimeSpan delay)
        {
            var builder = factory.Create<DelayScheduleBuilder>();
            builder.Delay = delay;

            return builder;
        }

        public static IScheduleBuilder Utc(this IScheduleBuilderFactory factory, DateTime utc)
        {
            var builder = factory.Create<UtcScheduleBuilder>();
            builder.Utc = utc;

            return builder;
        }

        public static CronTabScheduleBuilder Crontab(this IScheduleBuilderFactory factory, string crontab)
        {
            var builder = factory.Create<CronTabScheduleBuilder>();
            builder.Crontab = crontab;

            return builder;
        }

        public static IScheduleBuilder Repeat(this CronTabScheduleBuilder scheduleBuilder)
        {
            var builder = new RepeatFromCrontabScheduleBuilder(scheduleBuilder);

            return builder;
        }

        public static IScheduleBuilder Repeat(this IScheduleBuilder scheduleBuilder, string crontab)
        {
            var builder = new RepeatCrontabScheduleBuilder(scheduleBuilder, crontab);

            return builder;
        }
    }
}
