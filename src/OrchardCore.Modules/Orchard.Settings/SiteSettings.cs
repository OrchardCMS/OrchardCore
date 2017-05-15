using Microsoft.AspNetCore.Routing;
using Orchard.Entities;

namespace Orchard.Settings
{
    public class SiteSettings : Entity, ISite
    {
        public string BaseUrl { get; set; }
        public string Calendar { get; set; }
        public string Culture { get; set; }
        public int MaxPagedCount { get; set; }
        public int MaxPageSize { get; set; }
        public int PageSize { get; set; }
        public ResourceDebugMode ResourceDebugMode { get; set; }
        public string SiteName { get; set; }
        public string SiteSalt { get; set; }
        public string SuperUser { get; set; }
        public string TimeZone { get; set; }
        public bool UseCdn { get; set; }
        public RouteValueDictionary HomeRoute { get; set; }
    }
}