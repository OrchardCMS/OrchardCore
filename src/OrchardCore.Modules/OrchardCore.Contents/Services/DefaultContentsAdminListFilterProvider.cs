using System;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Records;
using OrchardCore.Contents.ViewModels;
using OrchardCore.Filters.Query;
using YesSql.Services;

namespace OrchardCore.Contents.Services
{
    public class DefaultContentsAdminListFilterProvider : IContentsAdminListFilterProvider
    {
        public void Build(QueryEngineBuilder<ContentItem> builder)
        {
            builder
                .WithNamedTerm("status", b => b
                    .OneCondition<ContentItem>((val, query) =>
                    {
                        if (Enum.TryParse<ContentsStatus>(val, true, out var e))
                        {
                            switch (e)
                            {
                                case ContentsStatus.Published:
                                    query.With<ContentItemIndex>(x => x.Published);
                                    break;
                                case ContentsStatus.Draft:
                                    query.With<ContentItemIndex>(x => x.Latest && !x.Published);
                                    break;
                                case ContentsStatus.AllVersions:
                                    query.With<ContentItemIndex>(x => x.Latest);
                                    break;
                                default:
                                    query.With<ContentItemIndex>(x => x.Latest);
                                    break;
                            }
                        }

                        return query;
                    })
                    .MapTo<ContentOptionsViewModel>((val, m) =>
                    {
                        if (Enum.TryParse<ContentsStatus>(val, true, out var contentsStatus))
                        {
                            m.ContentsStatus = contentsStatus;
                        }
                    })
                    .MapFrom<ContentOptionsViewModel>((m) =>
                    {
                        if (m.ContentsStatus != ContentsStatus.AllVersions)
                        {
                            return (true, m.ContentsStatus.ToString());
                        }

                        return (false, String.Empty);
                    })
                )
                .WithNamedTerm("sort", b => b
                    .OneCondition<ContentItem>((val, query) =>
                    {
                        // TODO we can also support -asc and -desc here.

                        var values = val.Split('-', 2);

                        var enumVal = values[0];
                        bool ascending = false;
                        if (values.Length == 2 && values[1] == "asc")
                        {
                            ascending = true;
                        }

                        if (Enum.TryParse<ContentsOrder>(val, true, out var contentsOrder))
                        {
                            switch (contentsOrder)
                            {
                                case ContentsOrder.Modified:
                                    if (ascending)
                                    {
                                        query.With<ContentItemIndex>().OrderBy(x => x.ModifiedUtc);
                                    }
                                    else
                                    {
                                        query.With<ContentItemIndex>().OrderByDescending(x => x.ModifiedUtc);
                                    }
                                    break;
                                case ContentsOrder.Published:
                                    if (ascending)
                                    {
                                        query.With<ContentItemIndex>().OrderBy(cr => cr.PublishedUtc);
                                    }
                                    else
                                    {
                                        query.With<ContentItemIndex>().OrderByDescending(cr => cr.PublishedUtc);
                                    }
                                    break;
                                case ContentsOrder.Created:
                                    if (ascending)
                                    {
                                        query.With<ContentItemIndex>().OrderBy(cr => cr.CreatedUtc);
                                    }
                                    else
                                    {
                                        query.With<ContentItemIndex>().OrderByDescending(cr => cr.CreatedUtc);
                                    }
                                    break;
                                case ContentsOrder.Title:
                                    // todo support ascending.
                                    query.With<ContentItemIndex>().OrderBy(cr => cr.DisplayText);
                                    break;

                                // todo add Default to the enum, or make it always modified. investigate.
                                default:
                                    query.With<ContentItemIndex>().OrderByDescending(cr => cr.ModifiedUtc);
                                    break;
                            };
                        }

                        return query;
                    })
                    .MapTo<ContentOptionsViewModel>((val, m) =>
                    {
                        if (Enum.TryParse<ContentsOrder>(val, true, out var e))
                        {
                            m.OrderBy = e;
                        }
                    })
                    .MapFrom<ContentOptionsViewModel>((m) =>
                    {
                        if (m.OrderBy != ContentsOrder.Modified)
                        {
                            return (true, m.OrderBy.ToString());
                        }

                        return (false, String.Empty);
                    })
                )
                .WithDefaultTerm("text", b => b
                        .ManyCondition<ContentItem>(
                            ((val, query) => query.With<ContentItemIndex>(x => x.DisplayText.Contains(val))),
                            ((val, query) => query.With<ContentItemIndex>(x => x.DisplayText.IsNotIn<ContentItemIndex>(s => s.DisplayText, w => w.DisplayText.Contains(val))))
                        )
                    );
        }
    }
}
