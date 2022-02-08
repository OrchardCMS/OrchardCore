using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.BackgroundJobs;
using OrchardCore.BackgroundJobs.Models;

namespace OrchardCore.BackgroundJobs.Services
{
    public class BackgroundJobOptions
    {
        private Dictionary<string, BackgroundJobOption> _backgroundJobs = new Dictionary<string, BackgroundJobOption>();
        public IReadOnlyDictionary<string, BackgroundJobOption> BackgroundJobs => _backgroundJobs;

        /// <summary>
        /// The number of times to retry a job after it fails.
        /// Defaults to 3.
        /// Can be customized on a per job basis <see cref="BackgroundJobOption.RetryAttempts"/>
        /// </summary>
        public int DefaultRetryAttempts { get; set; } = 3;

        /// <summary>
        /// The default retry interval.
        /// Defaults to 1 minute.
        /// Can be customized on a per job basis <see cref="BackgroundJobOption.RetryInterval"/>
        /// </summary>
        public TimeSpan DefaultRetryInterval { get; set; } = TimeSpan.FromMinutes(1);

        internal void AddBackgroundJob(Type backgroundJobType, Type backgroundJobHandler, Action<BackgroundJobOption> configure = null)
        {
            if (!backgroundJobType.IsAssignableTo(typeof(IBackgroundJob)))
            {
                throw new ArgumentException("The type must implement from " + nameof(IBackgroundJob));
            }

            if (!backgroundJobHandler.IsAssignableTo(typeof(IBackgroundJobHandler<>).MakeGenericType(backgroundJobType)))
            {
                throw new ArgumentException("The type must implement from " + nameof(IBackgroundJobHandler));
            }

            var option = new BackgroundJobOption(backgroundJobType, backgroundJobHandler);
            if (configure is not null)
            {
                configure(option);
            }

            _backgroundJobs[backgroundJobType.Name] = option;
        }

        internal void RemoveBackgroundJob(Type backgroundJobType)
            => _backgroundJobs.Remove(backgroundJobType.Name);
    }
}
