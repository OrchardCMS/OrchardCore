using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OrchardCore.Modules;
using OrchardCore.Modules.Manifest;
using OrchardCore.Recipes.Services;
using OrchardCore.Testing.Context;

namespace OrchardCore.Tests.Apis.Context
{
    public class SiteStartup : SiteStartupBase
    {
        protected override string[] TenantFeatures => new string[] { "OrchardCore.Apis.GraphQL" };

        public override Type WebStartupClass => typeof(Cms.Web.Startup);
    }
}
