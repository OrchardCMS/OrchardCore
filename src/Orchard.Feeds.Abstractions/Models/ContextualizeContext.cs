using System;
using Microsoft.AspNetCore.Mvc;

namespace Orchard.Feeds.Models
{
    public class ContextualizeContext
    {
        public IServiceProvider ServiceProvider { get; set; }
        public IUrlHelper Url { get; set; }
    }
}
