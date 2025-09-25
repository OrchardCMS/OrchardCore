using OrchardCore.Data.Documents;
using OrchardCore.Shortcodes.Services;
using OrchardCore.Shortcodes.ViewModels;
using YesSql.Filters.Query;

namespace OrchardCore.Contents.Services;

public sealed class DefaultShortcodesAdminListFilterProvider : IShortcodesAdminListFilterProvider
{
    public void Build(QueryEngineBuilder<IDocument> builder)
    {
        builder.WithNamedTerm(
            "sort",
            builder =>
                builder
                    .OneCondition(
                        (val, query) =>
                        {
                            if (Enum.TryParse<ContentsOrder>(val, true, out var contentsOrder))
                            {
                                switch (contentsOrder)
                                {
                                    case ContentsOrder.Modified:
                                        /*                                             query
                                                                                        .With<ContentItemIndex>()
                                                                                        .OrderByDescending(cr => cr.ModifiedUtc)
                                                                                        .ThenBy(cr => cr.Id); */
                                        break;
                                    case ContentsOrder.Created:
                                        /*                                             query
                                                                                        .With<ContentItemIndex>()
                                                                                        .OrderByDescending(cr => cr.CreatedUtc)
                                                                                        .ThenBy(cr => cr.Id); */
                                        break;
                                    case ContentsOrder.Title:
                                        /*                                             query
                                                                                        .With<ContentItemIndex>()
                                                                                        .OrderBy(cr => cr.DisplayText)
                                                                                        .ThenBy(cr => cr.Id); */
                                        break;
                                }
                            }
                            else
                            {
                                // Modified is a default value and applied when there is no filter.
                                /*                                     query
                                                                        .With<ContentItemIndex>()
                                                                        .OrderByDescending(cr => cr.ModifiedUtc)
                                                                        .ThenBy(cr => cr.Id); */
                            }

                            return query;
                        }
                    )
                    .MapTo<ShortcodeFilter>(
                        (val, model) =>
                        {
                            if (Enum.TryParse<ContentsOrder>(val, true, out var contentsOrder))
                            {
                                model.OrderBy = contentsOrder;
                            }
                        }
                    )
                    .MapFrom<ShortcodeFilter>(
                        (model) =>
                        {
                            if (model.OrderBy != ContentsOrder.Modified)
                            {
                                return (true, model.OrderBy.ToString());
                            }

                            return (false, string.Empty);
                        }
                    )
                    .AlwaysRun()
        );
    }
}
