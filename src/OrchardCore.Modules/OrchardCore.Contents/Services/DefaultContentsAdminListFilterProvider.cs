using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Records;
using OrchardCore.Contents.ViewModels;
using OrchardCore.Filters.Query;
using YesSql;
using YesSql.Services;

namespace OrchardCore.Contents.Services
{
    public class DefaultContentsAdminListFilterProvider : IContentsAdminListFilterProvider
    {
        public void Build(QueryEngineBuilder<ContentItem> builder)
        {
            builder
                .WithNamedTerm("status", builder => builder
                    .OneCondition<ContentItem>((val, query, ctx) =>
                    {
                        if (Enum.TryParse<ContentsStatus>(val, true, out var contentsStatus))
                        {
                            switch (contentsStatus)
                            {
                                case ContentsStatus.Published:
                                    query.With<ContentItemIndex>(x => x.Published);
                                    break;
                                case ContentsStatus.Draft:
                                    query.With<ContentItemIndex>(x => x.Latest && !x.Published);
                                    break;
                                case ContentsStatus.Owner:
                                    var httpContextAccessor = ctx.ServiceProvider.GetRequiredService<IHttpContextAccessor>();
                                    var userNameIdentifier = httpContextAccessor.HttpContext.User?.FindFirstValue(ClaimTypes.NameIdentifier);
                                    query.With<ContentItemIndex>(x => x.Owner == userNameIdentifier);
                                    break;
                                case ContentsStatus.AllVersions:
                                    query.With<ContentItemIndex>(x => x.Latest);
                                    break;
                                default:
                                    query.With<ContentItemIndex>(x => x.Latest);
                                    break;
                            }
                        }

                        return new ValueTask<IQuery<ContentItem>>(query);
                    })
                    .MapTo<ContentOptionsViewModel>((val, model) =>
                    {
                        if (Enum.TryParse<ContentsStatus>(val, true, out var contentsStatus))
                        {
                            model.ContentsStatus = contentsStatus;
                        }
                    })
                    .MapFrom<ContentOptionsViewModel>((model) =>
                    {
                        if (model.ContentsStatus != ContentsStatus.Latest)
                        {
                            return (true, model.ContentsStatus.ToString());
                        }

                        return (false, String.Empty);
                    })
                )
                .WithNamedTerm("sort", builder => builder
                    .OneCondition<ContentItem>((val, query) =>
                    {
                        if (Enum.TryParse<ContentsOrder>(val, true, out var contentsOrder))
                        {
                            switch (contentsOrder)
                            {
                                case ContentsOrder.Modified:
                                    query.With<ContentItemIndex>().OrderByDescending(x => x.ModifiedUtc);
                                    break;
                                case ContentsOrder.Published:
                                    query.With<ContentItemIndex>().OrderByDescending(cr => cr.PublishedUtc);
                                    break;
                                case ContentsOrder.Created:
                                    query.With<ContentItemIndex>().OrderByDescending(cr => cr.CreatedUtc);
                                    break;
                                case ContentsOrder.Title:
                                    query.With<ContentItemIndex>().OrderBy(cr => cr.DisplayText);
                                    break;
                            };
                        }
                        else
                        {
                            query.With<ContentItemIndex>().OrderByDescending(cr => cr.ModifiedUtc);
                        }

                        return query;
                    })
                    .MapTo<ContentOptionsViewModel>((val, model) =>
                    {
                        if (Enum.TryParse<ContentsOrder>(val, true, out var contentsOrder))
                        {
                            model.OrderBy = contentsOrder;
                        }
                    })
                    .MapFrom<ContentOptionsViewModel>((model) =>
                    {
                        if (model.OrderBy != ContentsOrder.Modified)
                        {
                            return (true, model.OrderBy.ToString());
                        }

                        return (false, String.Empty);
                    })
                )
                .WithDefaultTerm("text", builder => builder
                        .ManyCondition<ContentItem>(
                            ((val, query) => query.With<ContentItemIndex>(x => x.DisplayText.Contains(val))),
                            ((val, query) => query.With<ContentItemIndex>(x => x.DisplayText.IsNotIn<ContentItemIndex>(s => s.DisplayText, w => w.DisplayText.Contains(val))))
                        )
                    );
        }
    }
}
