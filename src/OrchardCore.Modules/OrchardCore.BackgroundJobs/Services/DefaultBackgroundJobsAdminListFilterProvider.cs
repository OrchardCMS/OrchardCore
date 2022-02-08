using System;
using OrchardCore.BackgroundJobs.Indexes;
using OrchardCore.BackgroundJobs.Models;
using OrchardCore.BackgroundJobs.ViewModels;
using YesSql.Filters.Query;

namespace OrchardCore.BackgroundJobs.Services
{
    public class DefaultBackgroundJobsAdminListFilterProvider : IBackgroundJobsAdminListFilterProvider
    {
        public void Build(QueryEngineBuilder<BackgroundJobExecution> builder)
        {
            builder
                .WithNamedTerm("correlationid", builder => builder
                    .OneCondition((val, query) =>
                    {
                        if (!String.IsNullOrEmpty(val))
                        {
                            query.With<BackgroundJobIndex>(x => x.CorrelationId == val);
                        }

                        return query;
                    })
                    .MapTo<BackgroundJobIndexOptions>((val, model) =>
                    {
                        model.CorrelationId = val;
                    })
                    .MapFrom<BackgroundJobIndexOptions>((model) =>
                    {
                        if (!String.IsNullOrEmpty(model.CorrelationId))
                        {
                            return (true, model.CorrelationId);
                        }

                        return (false, String.Empty);
                    })
                )
                .WithNamedTerm("repeatcorrelationid", builder => builder
                    .OneCondition((val, query) =>
                    {
                        if (!String.IsNullOrEmpty(val))
                        {
                            query.With<BackgroundJobIndex>(x => x.RepeatCorrelationId == val);
                        }

                        return query;
                    })
                    .MapTo<BackgroundJobIndexOptions>((val, model) =>
                    {
                        model.RepeatCorrelationId = val;
                    })
                    .MapFrom<BackgroundJobIndexOptions>((model) =>
                    {
                        if (!String.IsNullOrEmpty(model.RepeatCorrelationId))
                        {
                            return (true, model.RepeatCorrelationId);
                        }

                        return (false, String.Empty);
                    })
                )
                .WithNamedTerm("state", builder => builder
                     .OneCondition((val, query) =>
                     {
                         if (Enum.TryParse<BackgroundJobStatus>(val, true, out var jobState) && jobState != BackgroundJobStatus.Scheduled)
                         {
                             query.With<BackgroundJobIndex>(u => u.Status == jobState);

                         }

                         return query;
                     })
                     .MapTo<BackgroundJobIndexOptions>((val, model) =>
                     {
                         if (Enum.TryParse<BackgroundJobStatus>(val, true, out var jobState))
                         {
                             model.Filter = jobState;
                         }
                     })
                     .MapFrom<BackgroundJobIndexOptions>((model) =>
                     {
                         if (model.Filter != BackgroundJobStatus.Scheduled)
                         {
                             return (true, model.Filter.ToString());
                         }

                         return (false, String.Empty);
                     })
                );
            // TODO ther est of filters
        }
    }
}
