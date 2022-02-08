using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OrchardCore.BackgroundJobs;
using OrchardCore.BackgroundJobs.Models;
using OrchardCore.BackgroundJobs.Services;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection RemoveBackgroundJob<TJob>(this IServiceCollection services)
            where TJob : IBackgroundJob
            => services.RemoveBackgroundJob(typeof(TJob));

        public static IServiceCollection RemoveBackgroundJob(this IServiceCollection services, Type backgroundJobType)
            => services.Configure<BackgroundJobOptions>(o => o.RemoveBackgroundJob(backgroundJobType));

        public static IServiceCollection AddBackgroundJob<TJob, TJobHandler>(this IServiceCollection services, Action<BackgroundJobOption> configure = null)
            where TJob : IBackgroundJob
            where TJobHandler : IBackgroundJobHandler<TJob>
            => services.AddBackgroundJob(typeof(TJob), typeof(TJobHandler), configure);

        public static IServiceCollection AddBackgroundJob(this IServiceCollection services, Type backgroundJobType, Type backgroundJobHandler, Action<BackgroundJobOption> configure = null)
            => services.Configure<BackgroundJobOptions>(o => o.AddBackgroundJob(backgroundJobType, backgroundJobHandler, configure));
    }
}
