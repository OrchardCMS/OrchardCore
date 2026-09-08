using System.Security.Claims;
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

namespace OrchardCore.Contents.Services;

public sealed class DefaultContentsAdminListFilterProvider : IContentsAdminListFilterProvider
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

                    return ValueTask.FromResult<IQuery<ContentItem>>(query);
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

                    return (false, string.Empty);
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
                                query.With<ContentItemIndex>().OrderByDescending(cr => cr.ModifiedUtc).ThenBy(cr => cr.Id);
                                break;
                            case ContentsOrder.Published:
                                query.With<ContentItemIndex>().OrderByDescending(cr => cr.PublishedUtc).ThenBy(cr => cr.Id);
                                break;
                            case ContentsOrder.Created:
                                query.With<ContentItemIndex>().OrderByDescending(cr => cr.CreatedUtc).ThenBy(cr => cr.Id);
                                break;
                            case ContentsOrder.Title:
                                query.With<ContentItemIndex>().OrderBy(cr => cr.DisplayText).ThenBy(cr => cr.Id);
                                break;
                        }
                    }
                    else
                    {
                        // Modified is a default value and applied when there is no filter.
                        query.With<ContentItemIndex>().OrderByDescending(cr => cr.ModifiedUtc).ThenBy(cr => cr.Id);
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

                    return (false, string.Empty);
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

                    // Filter for one or more specific types.
                    if (!string.IsNullOrEmpty(contentType))
                    {
                        List<string> viewAll = null;
                        List<string> viewOwn = null;

                        foreach (var contentTypeId in contentType.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                        {
                            var contentTypeDefinition = await contentDefinitionManager.GetTypeDefinitionAsync(contentTypeId);
                            if (contentTypeDefinition == null)
                            {
                                continue;
                            }

                            // We display a specific type even if it's not listable so that admin pages
                            // can reuse the Content list page for specific types.

                            // It is important to pass null to the owner parameter. This will check if the user can view content that belongs to others.
                            if (await authorizationService.AuthorizeContentTypeAsync(user, CommonPermissions.ViewContent, contentTypeDefinition.Name, owner: null))
                            {
                                (viewAll ??= []).Add(contentTypeDefinition.Name);

                                continue;
                            }

                            (viewOwn ??= []).Add(contentTypeDefinition.Name);
                        }

                        if (viewAll != null || viewOwn != null)
                        {
                            if (viewAll == null)
                            {
                                return query.With<ContentItemIndex>(x => x.ContentType.IsIn(viewOwn) && x.Owner == userNameIdentifier);
                            }

                            if (viewOwn == null)
                            {
                                return query.With<ContentItemIndex>(x => x.ContentType.IsIn(viewAll));
                            }

                            return query.With<ContentItemIndex>(x => x.ContentType.IsIn(viewAll) || (x.ContentType.IsIn(viewOwn) && x.Owner == userNameIdentifier));
                        }

                        // At this point, the given content types are invalid. Ignore them.
                    }

                    List<string> listAnyContentTypes = null;
                    List<string> listOwnContentTypes = null;

                    foreach (var ctd in await contentDefinitionManager.ListTypeDefinitionsAsync())
                    {
                        if (!ctd.IsListable())
                        {
                            continue;
                        }

                        // It is important to pass null to the owner parameter. This will check if the user can view content that belongs to others.
                        if (await authorizationService.AuthorizeContentTypeAsync(user, CommonPermissions.ViewContent, ctd.Name, owner: null))
                        {
                            (listAnyContentTypes ??= []).Add(ctd.Name);

                            continue;
                        }

                        // It is important to pass the current user ID to the owner parameter. This will check if the user can view their own content.
                        if (await authorizationService.AuthorizeContentTypeAsync(user, CommonPermissions.ViewContent, ctd.Name, userNameIdentifier))
                        {
                            (listOwnContentTypes ??= []).Add(ctd.Name);

                            continue;
                        }
                    }

                    if (listAnyContentTypes == null)
                    {
                        return listOwnContentTypes == null
                            ? query.With<ContentItemIndex>().Where(x => false)
                            : query.With<ContentItemIndex>().Where(x => x.ContentType.IsIn(listOwnContentTypes) && x.Owner == userNameIdentifier);
                    }

                    if (listOwnContentTypes == null)
                    {
                        return query.With<ContentItemIndex>().Where(x => x.ContentType.IsIn(listAnyContentTypes));
                    }

                    return query.With<ContentItemIndex>().Where(x => x.ContentType.IsIn(listAnyContentTypes) || (x.ContentType.IsIn(listOwnContentTypes) && x.Owner == userNameIdentifier));
                })
                .MapTo<ContentOptionsViewModel>((val, model) =>
                {
                    if (!string.IsNullOrEmpty(val) && !val.Contains(','))
                    {
                        model.SelectedContentType = val;
                    }
                })
                .MapFrom<ContentOptionsViewModel>((model) =>
                {
                    if (!string.IsNullOrEmpty(model.SelectedContentType))
                    {
                        return (true, model.SelectedContentType);
                    }

                    return (false, string.Empty);
                })
                .AlwaysRun()
            )
            .WithNamedTerm("stereotype", builder => builder
                .OneCondition(async (stereotype, query, ctx) =>
                {
                    var context = (ContentQueryContext)ctx;
                    var httpContextAccessor = context.ServiceProvider.GetRequiredService<IHttpContextAccessor>();
                    var authorizationService = context.ServiceProvider.GetRequiredService<IAuthorizationService>();
                    var contentDefinitionManager = context.ServiceProvider.GetRequiredService<IContentDefinitionManager>();

                    // Filter for one or more stereotypes.
                    if (!string.IsNullOrEmpty(stereotype))
                    {
                        var stereotypes = stereotype.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                        var contentTypeDefinitionNames = (await contentDefinitionManager.ListTypeDefinitionsAsync())
                            .Where(definition => stereotypes.Any(value => definition.StereotypeEquals(value, StringComparison.OrdinalIgnoreCase)))
                            .Select(definition => definition.Name)
                            .ToList();

                        // We display a specific type even if it's not listable so that admin pages
                        // can reuse the content list page for specific types.

                        if (contentTypeDefinitionNames.Count > 0)
                        {
                            var user = httpContextAccessor.HttpContext.User;
                            var userNameIdentifier = user?.FindFirstValue(ClaimTypes.NameIdentifier);

                            List<string> viewAll = null;
                            List<string> viewOwn = null;

                            foreach (var contentTypeDefinitionName in contentTypeDefinitionNames)
                            {
                                // It is important to pass null to the owner parameter. This will check if the user can view content that belongs to others.
                                if (await authorizationService.AuthorizeContentTypeAsync(user, CommonPermissions.ViewContent, contentTypeDefinitionName, owner: null))
                                {
                                    (viewAll ??= []).Add(contentTypeDefinitionName);

                                    continue;
                                }

                                (viewOwn ??= []).Add(contentTypeDefinitionName);
                            }

                            if (viewAll == null)
                            {
                                return query.With<ContentItemIndex>(x => x.ContentType.IsIn(viewOwn) && x.Owner == userNameIdentifier);
                            }

                            if (viewOwn == null)
                            {
                                return query.With<ContentItemIndex>(x => x.ContentType.IsIn(viewAll));
                            }

                            return query.With<ContentItemIndex>(x => x.ContentType.IsIn(viewAll) || (x.ContentType.IsIn(viewOwn) && x.Owner == userNameIdentifier));
                        }

                        // At this point, the given stereotypes are invalid. Ignore them.
                    }

                    return query;
                })
            )
            .WithDefaultTerm(ContentsAdminListFilterOptions.DefaultTermName, builder => builder
                .ManyCondition(
                    (val, query) => query.With<ContentItemIndex>(x => x.DisplayText.Contains(val)),
                    (val, query) => query.With<ContentItemIndex>(x => x.DisplayText.NotContains(val))
                )
            );
    }
}
