using System;
using OrchardCore.ContentLocalization.Records;
using OrchardCore.ContentLocalization.ViewModels;
using OrchardCore.ContentManagement;
using OrchardCore.Contents.Services;
using YesSql.Filters.Query;

namespace OrchardCore.ContentLocalization.Services
{
    public class LocalizationPartContentsAdminListFilterProvider : IContentsAdminListFilterProvider
    {
        public void Build(QueryEngineBuilder<ContentItem> builder)
        {
            builder
                .WithNamedTerm("culture", builder => builder
                    .OneCondition<ContentItem>((val, query) =>
                    {
                        if (!String.IsNullOrEmpty(val))
                        {
                            query.With<LocalizedContentItemIndex>(i => (i.Published || i.Latest) && i.Culture == val);
                        }

                        return query;
                    })
                    .MapTo<LocalizationContentsAdminFilterViewModel>((val, model) => model.SelectedCulture = val)
                    .MapFrom<LocalizationContentsAdminFilterViewModel>((model) =>
                    {
                        if (!String.IsNullOrEmpty(model.SelectedCulture))
                        {
                            return (true, model.SelectedCulture);
                        }
                        return (false, String.Empty);
                    })
                );
        }
    }
}
