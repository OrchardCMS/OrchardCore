using System.Linq;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Records;
using OrchardCore.Contents.Core.Options;
using YesSql;
using YesSql.Services;

namespace OrchardCore.Contents.Services
{
    public class ContentQueryService : IContentQueryService
    {
        private readonly YesSql.ISession _session;

        public ContentQueryService(YesSql.ISession session)
        {
            _session = session;
        }
        public virtual IQuery<ContentItem, ContentItemIndex> GetQueryByOptions(OrchardCore.Contents.Core.Options.ContentOptions options)
        {
            var query = _session.Query<ContentItem, ContentItemIndex>();

            if (!string.IsNullOrEmpty(options.DisplayText))
            {
                query = query.With<ContentItemIndex>(x => x.DisplayText.Contains(options.DisplayText));
            }

            switch (options.ContentsStatus)
            {
                case ContentsStatus.Published:
                    query = query.With<ContentItemIndex>(x => x.Published);
                    break;
                case ContentsStatus.Draft:
                    query = query.With<ContentItemIndex>(x => x.Latest && !x.Published);
                    break;
                case ContentsStatus.AllVersions:
                    query = query.With<ContentItemIndex>(x => x.Latest);
                    break;
                case ContentsStatus.Owner:
                    if (!string.IsNullOrEmpty(options.OwnerName))
                    {
                        query = query.With<ContentItemIndex>(x => x.Owner == options.OwnerName);
                    }
                    break;
                default:
                    query = query.With<ContentItemIndex>(x => x.Latest);
                    break;
            }

            if (!string.IsNullOrEmpty(options.SelectedContentType))
            {
                // We display a specific type even if it's not listable so that admin pages
                // can reuse the Content list page for specific types.
                query = query.With<ContentItemIndex>(x => x.ContentType == options.SelectedContentType);
            }
            else
            {
                //var listableTypes = (await GetListableTypesAsync(user)).Select(t => t.Name).ToArray();
                if (options.ListableContentTypes != null && options.ListableContentTypes.Any())
                {
                    query = query.With<ContentItemIndex>(x => x.ContentType.IsIn(options.ListableContentTypes));
                }
            }

            switch (options.OrderBy)
            {
                case ContentsOrder.Modified:
                    query = query.OrderByDescending(x => x.ModifiedUtc);
                    break;
                case ContentsOrder.Published:
                    query = query.OrderByDescending(cr => cr.PublishedUtc);
                    break;
                case ContentsOrder.Created:
                    query = query.OrderByDescending(cr => cr.CreatedUtc);
                    break;
                case ContentsOrder.Title:
                    query = query.OrderBy(cr => cr.DisplayText);
                    break;
                default:
                    query = query.OrderByDescending(cr => cr.ModifiedUtc);
                    break;
            }

            return query;
        }
    }
}
