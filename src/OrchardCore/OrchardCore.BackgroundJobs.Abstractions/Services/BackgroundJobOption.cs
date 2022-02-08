using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;

namespace OrchardCore.BackgroundJobs.Services
{
    public class BackgroundJobOption
    {
        private ObjectFactory _factory;

        public BackgroundJobOption(Type backgroundJobType, Type backgroundJobHandlerType)
        {
            BackgroundJobType = backgroundJobType;
            BackgroundJobHandlerType = backgroundJobHandlerType;
        }

        public Type BackgroundJobType { get; }
        public Type BackgroundJobHandlerType { get; }
        public ObjectFactory HandlerFactory => _factory ??= ActivatorUtilities.CreateFactory(BackgroundJobHandlerType, Type.EmptyTypes);

        /// <summary>
        /// Localized display name accessor.
        /// </summary>
        public Func<IServiceProvider, LocalizedString> DisplayNameAccessor { get; internal set; }

        /// <summary>
        /// The number of times to retry a job after it fails.
        /// When <see langword="null"/> will use the <see cref="BackgroundJobOptions.DefaultRetryAttempts"/>
        /// </summary>
        public int? RetryAttempts { get; set; }

        /// <summary>
        /// The number of times to retry a job after it fails.
        /// When <see langword="null"/> will use the <see cref="BackgroundJobOptions.DefaultRetryAttempts"/>
        /// </summary>
        public TimeSpan? RetryInterval { get; set; }

        /// <summary>
        /// The number of parallel jobs of this type allowed.
        /// NB. This is a per tenant concurrent jobs limit.
        /// When 0 is unlimited.
        /// </summary>
        public int ConcurrentJobsLimit { get; set; } = 1;

        // TODO implement timeout.
    }

    public static class BackgroundJobOptionExtensions
    {
        public static void WithDisplayName<TLocalizer>(this BackgroundJobOption option, Func<IStringLocalizer, LocalizedString> displayName) where TLocalizer : class
        {
            option.DisplayNameAccessor = (sp) => displayName((IStringLocalizer)sp.GetService(typeof(IStringLocalizer<>).MakeGenericType(typeof(TLocalizer))));
        }
    }
}
