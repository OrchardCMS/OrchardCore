using System;
using System.Linq;
using DotLiquid;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;
using YesSql;

namespace Orchard.Liquid.Drops
{
    public class ContentQueryDrop : Drop, IIndexable
    {
        private readonly IContentManager _contentManager;
        private readonly ISession _session;

        public ContentQueryDrop(IContentManager contentManager, ISession session)
        {
            _contentManager = contentManager;
            _session = session;
            Query = _session.Query<ContentItem, ContentItemIndex>().Where(x => x.Published);
        }

        public ContentTypeDrop ContentType => new ContentTypeDrop(this);
        public OrderByDrop OrderBy => new OrderByDrop(this);
        public OrderByDescendingDrop OrderByDescending => new OrderByDescendingDrop(this);
        public WhereDrop Where => new WhereDrop(this);
        public SkipDrop Skip => new SkipDrop(this);
        public TakeDrop Take => new TakeDrop(this);
        
        public IQuery<ContentItem, ContentItemIndex> Query { get; set; }

        public object List
        {
            get
            {
                var result = Query.ListAsync().GetAwaiter().GetResult().ToList();
                return result;
            }
        }
    }

    public class ContentTypeDrop : Drop, IIndexable
    {
        private ContentQueryDrop _query;

        public ContentTypeDrop(ContentQueryDrop query)
        {
            _query = query;
        }

        object IIndexable.this[object key]
        {
            get
            {
                _query.Query = _query.Query.Where(x => x.ContentType == key.ToString());
                return _query;
            }
        }

        bool IIndexable.ContainsKey(object key)
        {
            return true;
        }
    }

    public class OrderByDrop : Drop, IIndexable
    {
        private ContentQueryDrop _query;

        public OrderByDrop(ContentQueryDrop query)
        {
            _query = query;
        }

        object IIndexable.this[object key]
        {
            get
            {
                switch (Convert.ToString(key))
                {
                    case "Created": _query.Query = _query.Query.OrderBy(x => x.CreatedUtc); break;
                    case "Modified": _query.Query = _query.Query.OrderBy(x => x.ModifiedUtc); break;
                    case "Published": _query.Query = _query.Query.OrderBy(x => x.PublishedUtc); break;
                    case "Id": _query.Query = _query.Query.OrderBy(x => x.Id); break;
                    case "ContentItemId": _query.Query = _query.Query.OrderBy(x => x.ContentItemId); break;
                }
                return _query;
            }
        }

        bool IIndexable.ContainsKey(object key)
        {
            return true;
        }
    }

    public class OrderByDescendingDrop : Drop, IIndexable
    {
        private ContentQueryDrop _query;

        public OrderByDescendingDrop(ContentQueryDrop query)
        {
            _query = query;
        }

        object IIndexable.this[object key]
        {
            get
            {
                switch (Convert.ToString(key))
                {
                    case "Created": _query.Query = _query.Query.OrderByDescending(x => x.CreatedUtc); break;
                    case "Modified": _query.Query = _query.Query.OrderByDescending(x => x.ModifiedUtc); break;
                    case "Published": _query.Query = _query.Query.OrderByDescending(x => x.PublishedUtc); break;
                    case "Id": _query.Query = _query.Query.OrderByDescending(x => x.Id); break;
                    case "ContentItemId": _query.Query = _query.Query.OrderByDescending(x => x.ContentItemId); break;
                }
                return _query;
            }
        }

        bool IIndexable.ContainsKey(object key)
        {
            return true;
        }
    }

    public class WhereDrop : Drop, IIndexable
    {
        private ContentQueryDrop _query;

        public WhereDrop(ContentQueryDrop query)
        {
            _query = query;
        }

        // TODO: Secure
        object IIndexable.this[object key]
        {
            get
            {
                _query.Query = _query.Query.Where(Convert.ToString(key));
                return _query;
            }
        }

        bool IIndexable.ContainsKey(object key)
        {
            return true;
        }
    }

    public class TakeDrop : Drop, IIndexable
    {
        private ContentQueryDrop _query;

        public TakeDrop(ContentQueryDrop query)
        {
            _query = query;
        }

        object IIndexable.this[object key]
        {
            get
            {
                _query.Query = _query.Query.Take(Convert.ToInt32(key)).With<ContentItemIndex>();
                return _query;
            }
        }

        bool IIndexable.ContainsKey(object key)
        {
            return true;
        }
    }

    public class SkipDrop : Drop, IIndexable
    {
        private ContentQueryDrop _query;

        public SkipDrop(ContentQueryDrop query)
        {
            _query = query;
        }

        object IIndexable.this[object key]
        {
            get
            {
                _query.Query = _query.Query.Skip(Convert.ToInt32(key)).With<ContentItemIndex>();
                return _query;
            }
        }

        bool IIndexable.ContainsKey(object key)
        {
            return true;
        }
    }
}
