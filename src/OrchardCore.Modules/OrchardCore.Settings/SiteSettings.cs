using System;
using Microsoft.AspNetCore.Routing;
using OrchardCore.Entities;

namespace OrchardCore.Settings
{
    public class SiteSettings : Entity, ISite
    {
        public int Id { get; set; }
        public string BaseUrl { get; set; }
        public string Calendar { get; set; }
        public string Culture { get; set; }
        public string[] SupportedCultures { get; set; } = Array.Empty<string>();
        public int MaxPagedCount { get; set; }
        public int MaxPageSize { get; set; }
        public int PageSize { get; set; }
        public string TimeZoneId { get; set; }
        public ResourceDebugMode ResourceDebugMode { get; set; }
        public string SiteName { get; set; }
        public string SiteSalt { get; set; }
        public string SuperUser { get; set; }
        public bool UseCdn { get; set; }
        public RouteValueDictionary HomeRoute { get; set; } = new RouteValueDictionary();
    }
}