using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentManagement.Records;
using OrchardCore.Contents.ViewModels;
using YesSql;
using YesSql.Filters.Query;
using YesSql.Services;

namespace OrchardCore.Contents.Services
{
    public class DefaultContentsAdminListFilterProvider : IContentsAdminListFilterProvider
    {
        public void Build(QueryEngineBuilder<ContentItem> builder)
        {
            builder
                .WithNamedTerm("status", builder => builder
                    .OneCondition((val, query, ctx) =>
                    {
                        var context = (ContentQueryContext)ctx;
                        if (Enum.TryParse<ContentsStatus>(val, true, out var contentsStatus))
                        {
                            switch (contentsStatus)
                            {
                                case ContentsStatus.Draft:
                                    query.With<ContentItemIndex>(x => x.Latest && !x.Published);
                                    break;
                                case ContentsStatus.Published:
                                    query.With<ContentItemIndex>(x => x.Published);
                                    break;
                                case ContentsStatus.Owner:
                                    var httpContextAccessor = context.ServiceProvider.GetRequiredService<IHttpContextAccessor>();
                                    var userNameIdentifier = httpContextAccessor.HttpContext.User?.FindFirstValue(ClaimTypes.NameIdentifier);
                                    query.With<ContentItemIndex>(x => x.Owner == userNameIdentifier && x.Latest);
                                    break;
                                case ContentsStatus.AllVersions:
                                    query.With<ContentItemIndex>(x => x.Latest);
                                    break;
                                default:
                                    query.With<ContentItemIndex>(x => x.Latest);
                                    break;
                            }
                        }
                        else
                        {
                            // Draft is the default value.
                            query.With<ContentItemIndex>(x => x.Latest);
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
                    .AlwaysRun()
                )
                .WithNamedTerm("sort", builder => builder
                    .OneCondition((val, query) =>
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
                            // Modified is a default value and applied when there is no filter.
                            query.With<ContentItemIndex>().OrderByDescending(x => x.ModifiedUtc);
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
                    .AlwaysRun()
                )
                .WithNamedTerm("type", builder => builder
                    .OneCondition(async (contentType, query, ctx) =>
                    {
                        var context = (ContentQueryContext)ctx;
                        var httpContextAccessor = context.ServiceProvider.GetRequiredService<IHttpContextAccessor>();
                        var authorizationService = context.ServiceProvider.GetRequiredService<IAuthorizationService>();
                        var contentDefinitionManager = context.ServiceProvider.GetRequiredService<IContentDefinitionManager>();
                        var user = httpContextAccessor.HttpContext.User;
                        var userNameIdentifier = user?.FindFirstValue(ClaimTypes.NameIdentifier);

                        // Filter for a specific type.
                        if (!String.IsNullOrEmpty(contentType))
                        {
                            var contentTypeDefinition = contentDefinitionManager.GetTypeDefinition(contentType);
                            if (contentTypeDefinition != null)
                            {
                                // We display a specific type even if it's not listable so that admin pages
                                // can reuse the Content list page for specific types.

                                // It is important to pass null to the owner parameter. This will check if the user can view content that belongs to others.
                                if (await authorizationService.AuthorizeContentTypeAsync(user, CommonPermissions.ViewContent, contentTypeDefinition.Name, owner: null))
                                {
                                    return query.With<ContentItemIndex>(x => x.ContentType == contentType);
                                }

                                return query.With<ContentItemIndex>(x => x.ContentType == contentType && x.Owner == userNameIdentifier);
                            }
                            // At this point the given contentType is invalid. Ignore it.
                        }

                        var listAnyContentTypes = new List<string>();
                        var listOwnContentTypes = new List<string>();

                        foreach (var ctd in contentDefinitionManager.ListTypeDefinitions())
                        {
                            if (!ctd.IsListable())
                            {
                                continue;
                            }

                            // It is important to pass null to the owner parameter. This will check if the user can view content that belongs to others.
                            if (await authorizationService.AuthorizeContentTypeAsync(user, CommonPermissions.ViewContent, ctd.Name, owner: null))
                            {
                                listAnyContentTypes.Add(ctd.Name);

                                continue;
                            }

                            // It is important to pass the current user ID to the owner parameter. This will check if the user can view their own content.
                            if (await authorizationService.AuthorizeContentTypeAsync(user, CommonPermissions.ViewContent, ctd.Name, userNameIdentifier))
                            {
                                listOwnContentTypes.Add(ctd.Name);

                                continue;
                            }
                        }

                        return query.With<ContentItemIndex>().Where(x => x.ContentType.IsIn(listAnyContentTypes) || (x.ContentType.IsIn(listOwnContentTypes) && x.Owner == userNameIdentifier));
                    })
                    .MapTo<ContentOptionsViewModel>((val, model) =>
                    {
                        if (!String.IsNullOrEmpty(val))
                        {
                            model.SelectedContentType = val;
                        }
                    })
                    .MapFrom<ContentOptionsViewModel>((model) =>
                    {
                        if (!String.IsNullOrEmpty(model.SelectedContentType))
                        {
                            return (true, model.SelectedContentType);
                        }

                        return (false, String.Empty);
                    })
                    .AlwaysRun()
                )
                .WithDefaultTerm("text", builder => builder
                    .ManyCondition(
                        (val, query) => query.With<ContentItemIndex>(x => x.DisplayText.Contains(val)),
                        (val, query) => query.With<ContentItemIndex>(x => x.DisplayText.NotContains(val))
                    )
                );
        }
    }
}
