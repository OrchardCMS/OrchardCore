using System;
using DotLiquid;
using Microsoft.AspNetCore.Mvc;

namespace Orchard.Liquid.Drops
{
    public class UrlDrop : Drop
    {
        public UrlDrop(IUrlHelper urlHelper)
        {
            Url = urlHelper;
        }

        public IUrlHelper Url { get; set; }

        public UrlContentDrop Content => new UrlContentDrop(this);
    }

    public class UrlContentDrop : Drop, IIndexable
    {
        private UrlDrop _url;

        public UrlContentDrop(UrlDrop url)
        {
            _url = url;
        }

        object IIndexable.this[object key] => _url.Url.Content(Convert.ToString(key));

        bool IIndexable.ContainsKey(object key)
        {
            return true;
        }
    }
}
