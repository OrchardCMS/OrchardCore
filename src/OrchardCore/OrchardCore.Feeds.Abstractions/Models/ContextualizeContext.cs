using System;
using Microsoft.AspNetCore.Mvc;

namespace OrchardCore.Feeds.Models
{
    public class ContextualizeContext
    {
        public IServiceProvider ServiceProvider { get; set; }
        public IUrlHelper Url { get; set; }
    }
}
